using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Kidzgo.Application.LeaveRequests.CreateLeaveRequest;

public sealed class CreateLeaveRequestCommandHandler(IDbContext context)
    : ICommandHandler<CreateLeaveRequestCommand, CreateLeaveRequestResponse>
{
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

        // Auto approve path -> create makeup credit
        if (status == LeaveRequestStatus.Approved)
        {
            // Find a session of this class that matches the requested date
            Guid? sourceSessionId = await context.Sessions
                .Where(s => s.ClassId == command.ClassId &&
                            DateOnly.FromDateTime(s.PlannedDatetime) == command.SessionDate)
                .Select(s => (Guid?)s.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (sourceSessionId is null)
            {
                return Result.Failure<CreateLeaveRequestResponse>(
                    LeaveRequestErrors.SessionNotFound(command.ClassId, command.SessionDate));
            }

            var credit = new MakeupCredit
            {
                Id = Guid.NewGuid(),
                StudentProfileId = leave.StudentProfileId,
                SourceSessionId = sourceSessionId.Value,
                Status = MakeupCreditStatus.Available,
                CreatedReason = CreatedReason.ApprovedLeave24H,
                ExpiresAt = null,
                CreatedAt = DateTime.UtcNow
            };
            context.MakeupCredits.Add(credit);
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
            Status = leave.Status,
            RequestedAt = leave.RequestedAt,
            ApprovedAt = leave.ApprovedAt
        };
    }
}

