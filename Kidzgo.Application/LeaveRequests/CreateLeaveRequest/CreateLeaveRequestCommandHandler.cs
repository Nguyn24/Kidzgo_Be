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

        var sessionMonth = command.SessionDate.Month;
        var sessionYear = command.SessionDate.Year;
        var endDate = command.EndDate ?? command.SessionDate;

        // Get all sessions in the date range for this class
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

        // Get unique session dates in the range
        var sessionDatesInRange = sessionsInRange
            .Select(s => DateOnly.FromDateTime(s.PlannedDatetime))
            .Distinct()
            .ToList();

        // Get existing leave requests (Pending + Approved) for this student, class, and month
        var existingLeavesInMonth = await context.LeaveRequests
            .Where(lr => lr.StudentProfileId == command.StudentProfileId
                        && lr.ClassId == command.ClassId
                        && lr.SessionDate.Month == sessionMonth
                        && lr.SessionDate.Year == sessionYear
                        && (lr.Status == LeaveRequestStatus.Pending || lr.Status == LeaveRequestStatus.Approved)
                        && lr.Class.Status != ClassStatus.Closed)
            .ToListAsync(cancellationToken);

        // Count unique session dates from existing leave requests
        var existingSessionDates = new HashSet<DateOnly>();
        foreach (var leave in existingLeavesInMonth)
        {
            existingSessionDates.Add(leave.SessionDate);
        }

        // Count total unique session dates (existing + new request)
        var totalSessionDatesInMonth = existingSessionDates.Count + sessionDatesInRange.Count;

        if (totalSessionDatesInMonth > MaxLeavesPerMonth)
        {
            return Result.Failure<CreateLeaveRequestResponse>(LeaveRequestErrors.ExceededMonthlyLeaveLimit(MaxLeavesPerMonth));
        }

        // Compute notice hours from now until session date (first day)
        var firstDate = command.SessionDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var noticeHours = (int)Math.Floor((firstDate - DateTime.UtcNow).TotalHours);

        var status = noticeHours >= 24 ? LeaveRequestStatus.Approved : LeaveRequestStatus.Pending;

        var createdLeaves = new List<LeaveRequest>();

        // Create one LeaveRequest per session date (not per session)
        // sessionDate và endDate cách nhau bao nhiêu ngày thì tạo bấy nhiêu LeaveRequest
        foreach (var sessionDate in sessionDatesInRange)
        {
            // Compute notice hours for each session date
            var sessionDateTime = sessionDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var noticeHoursAfter = (int)Math.Floor((sessionDateTime - DateTime.UtcNow).TotalHours);

            var leave = new LeaveRequest
            {
                Id = Guid.NewGuid(),
                StudentProfileId = command.StudentProfileId,
                ClassId = command.ClassId,
                SessionDate = sessionDate,
                EndDate = null, // Each leave request is for a single day
                Reason = command.Reason,
                NoticeHours = noticeHoursAfter,
                Status = status,
                RequestedAt = DateTime.UtcNow
            };

            context.LeaveRequests.Add(leave);
            createdLeaves.Add(leave);
        }

        // Auto approve path -> create makeup credits and schedule makeup sessions on T7/CN
        if (status == LeaveRequestStatus.Approved)
        {
            foreach (var session in sessionsInRange)
            {
                var credit = new MakeupCredit
                {
                    Id = Guid.NewGuid(),
                    StudentProfileId = command.StudentProfileId,
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

            foreach (var leave in createdLeaves)
            {
                leave.ApprovedAt = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        // Return list of created leave requests
        return new CreateLeaveRequestResponse
        {
            LeaveRequests = createdLeaves.Select(l => new LeaveRequestItem
            {
                Id = l.Id,
                StudentProfileId = l.StudentProfileId,
                ClassId = l.ClassId,
                SessionDate = l.SessionDate,
                EndDate = l.EndDate,
                Reason = l.Reason,
                NoticeHours = l.NoticeHours,
                Status = l.Status.ToString(),
                RequestedAt = l.RequestedAt,
                ApprovedAt = l.ApprovedAt
            }).ToList()
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

