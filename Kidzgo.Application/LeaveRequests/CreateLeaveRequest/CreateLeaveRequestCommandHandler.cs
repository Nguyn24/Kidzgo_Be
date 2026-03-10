using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Kidzgo.Domain.Schools;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Kidzgo.Application.LeaveRequests.CreateLeaveRequest;

public sealed class CreateLeaveRequestCommandHandler(IDbContext context)
    : ICommandHandler<CreateLeaveRequestCommand, CreateLeaveRequestResponse>
{
    private const int MaxLeavesPerMonth = 2;

    public async Task<Result<CreateLeaveRequestResponse>> Handle(CreateLeaveRequestCommand command, CancellationToken cancellationToken)
    {
        var profile = await context.Profiles
            .Include(p => p.ClassEnrollments)
            .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId && !p.IsDeleted && p.IsActive, cancellationToken);

        if (profile is null)
        {
            return Result.Failure<CreateLeaveRequestResponse>(LeaveRequestErrors.NotFound(command.StudentProfileId));
        }

        // Basic enrollment check (optional)
        bool enrolled = profile.ClassEnrollments.Any(e => e.ClassId == command.ClassId && e.Status == EnrollmentStatus.Active);
        if (!enrolled)
        {
            return Result.Failure<CreateLeaveRequestResponse>(LeaveRequestErrors.NotEnrolled(
                command.ClassId,
                command.StudentProfileId));
        }

        // Get the class to check level and branch
        var classInfo = await context.Classes
            .Include(c => c.Program)
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);

        if (classInfo is null)
        {
            return Result.Failure<CreateLeaveRequestResponse>(LeaveRequestErrors.ClassNotFound(command.ClassId));
        }

        // Check monthly leave limit (Pending + Approved in the same month)
        var sessionMonth = command.SessionDate.Month;
        var sessionYear = command.SessionDate.Year;
        var endDate = command.EndDate ?? command.SessionDate;

        var leavesInMonth = await context.LeaveRequests
            .Where(lr => lr.StudentProfileId == command.StudentProfileId
                        && lr.SessionDate.Month == sessionMonth
                        && lr.SessionDate.Year == sessionYear
                        && (lr.Status == LeaveRequestStatus.Pending || lr.Status == LeaveRequestStatus.Approved)
                        && lr.Class.Status != ClassStatus.Closed)
            .CountAsync(cancellationToken);

        // Count days in the request range
        var daysInRange = (endDate.DayNumber - command.SessionDate.DayNumber) + 1;
        var totalLeavesInMonth = leavesInMonth + daysInRange;

        if (totalLeavesInMonth > MaxLeavesPerMonth)
        {
            return Result.Failure<CreateLeaveRequestResponse>(LeaveRequestErrors.ExceededMonthlyLeaveLimit(MaxLeavesPerMonth));
        }

        // Compute notice hours from now until session date (first day)
        var firstDate = command.SessionDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var noticeHours = (int)Math.Floor((firstDate - DateTime.UtcNow).TotalHours);

        var status = noticeHours >= 24 ? LeaveRequestStatus.Approved : LeaveRequestStatus.Pending;

        var leave = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            StudentProfileId = command.StudentProfileId,
            ClassId = command.ClassId,
            SessionDate = command.SessionDate,
            EndDate = command.EndDate,
            Reason = command.Reason,
            NoticeHours = noticeHours,
            Status = status,
            RequestedAt = DateTime.UtcNow
        };

        context.LeaveRequests.Add(leave);

        // Auto approve path -> create makeup credits and schedule makeup sessions on T7/CN
        if (status == LeaveRequestStatus.Approved)
        {
            var sessionsInRange = await context.Sessions
                .Where(s => s.ClassId == command.ClassId
                            && DateOnly.FromDateTime(s.PlannedDatetime) >= command.SessionDate
                            && DateOnly.FromDateTime(s.PlannedDatetime) <= endDate)
                .ToListAsync(cancellationToken);

            if (!sessionsInRange.Any())
            {
                return Result.Failure<CreateLeaveRequestResponse>(
                    LeaveRequestErrors.SessionNotFound(command.ClassId, command.SessionDate));
            }

            foreach (var session in sessionsInRange)
            {
                var credit = new MakeupCredit
                {
                    Id = Guid.NewGuid(),
                    StudentProfileId = leave.StudentProfileId,
                    SourceSessionId = session.Id,
                    Status = MakeupCreditStatus.Available,
                    CreatedReason = CreatedReason.ApprovedLeave24H,
                    ExpiresAt = null,
                    CreatedAt = DateTime.UtcNow
                };
                context.MakeupCredits.Add(credit);

                // Auto-schedule makeup session on T7/CN of the same week
                await ScheduleMakeupSessionAsync(context, credit, classInfo, session, cancellationToken);
            }

            leave.ApprovedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new CreateLeaveRequestResponse
        {
            Id = leave.Id,
            StudentProfileId = leave.StudentProfileId,
            ClassId = leave.ClassId,
            SessionDate = leave.SessionDate,
            EndDate = leave.EndDate,
            Reason = leave.Reason,
            NoticeHours = leave.NoticeHours,
            Status = leave.Status.ToString(),
            RequestedAt = leave.RequestedAt,
            ApprovedAt = leave.ApprovedAt
        };
    }

    private async Task ScheduleMakeupSessionAsync(
        IDbContext context,
        MakeupCredit credit,
        Class originalClass,
        Session originalSession,
        CancellationToken cancellationToken)
    {
        // Find Saturday and Sunday of the week when the student took leave
        var sessionDate = DateOnly.FromDateTime(originalSession.PlannedDatetime);
        var dayOfWeek = (int)sessionDate.DayOfWeek;
        
        // Calculate Saturday (day 6) and Sunday (day 0) of that week
        var saturday = sessionDate.AddDays(6 - dayOfWeek);
        var sunday = sessionDate.AddDays(-dayOfWeek);

        // Find T7/CN sessions in the same week, same level, same branch, different class
        var makeupSessions = await context.Sessions
            .Include(s => s.Class)
            .ThenInclude(c => c.Program)
            .Include(s => s.Attendances)
            .Where(s => s.BranchId == originalClass.BranchId
                        && s.ClassId != originalClass.Id
                        && s.Class.Status == ClassStatus.Active
                        && (DateOnly.FromDateTime(s.PlannedDatetime) == saturday 
                            || DateOnly.FromDateTime(s.PlannedDatetime) == sunday))
            .ToListAsync(cancellationToken);

        // Filter sessions with available slot (enrolled count < capacity)
        var availableSessions = makeupSessions
            .Where(s => s.Attendances.Count < s.Class.Capacity)
            .ToList();

        if (!availableSessions.Any())
        {
            return; // No available makeup session
        }

        // Random select one session
        var random = new Random();
        var selectedSession = availableSessions[random.Next(availableSessions.Count)];

        // Create makeup allocation
        var allocation = new MakeupAllocation
        {
            Id = Guid.NewGuid(),
            MakeupCreditId = credit.Id,
            TargetSessionId = selectedSession.Id,
            Status = MakeupAllocationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        context.MakeupAllocations.Add(allocation);
    }
}

