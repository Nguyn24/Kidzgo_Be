using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LeaveRequests.ApproveLeaveRequest;

public sealed class ApproveLeaveRequestCommandHandler(IDbContext context, IUserContext userContext)
    : ICommandHandler<ApproveLeaveRequestCommand>
{
    public async Task<Result> Handle(ApproveLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leave = await context.LeaveRequests
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (leave is null)
        {
            return Result.Failure(LeaveRequestErrors.NotFound(request.Id));
        }

        if (leave.Status == LeaveRequestStatus.Approved)
        {
            return Result.Failure(LeaveRequestErrors.AlreadyApproved);
        }

        leave.Status = LeaveRequestStatus.Approved;
        leave.ApprovedAt = DateTime.UtcNow;
        leave.ApprovedBy = userContext.UserId;

        // Tạo MakeupCredit cho tất cả các buổi học trong khoảng ngày xin nghỉ
        var endDate = leave.EndDate ?? leave.SessionDate;

        var sessionsInRange = await context.Sessions
            .Where(s => s.ClassId == leave.ClassId
                        && DateOnly.FromDateTime(s.PlannedDatetime) >= leave.SessionDate
                        && DateOnly.FromDateTime(s.PlannedDatetime) <= endDate)
            .ToListAsync(cancellationToken);

        if (!sessionsInRange.Any())
        {
            return Result.Failure(LeaveRequestErrors.SessionNotFound(leave.ClassId, leave.SessionDate));
        }

        foreach (var session in sessionsInRange)
        {
            bool creditExists = await context.MakeupCredits
                .AnyAsync(c => c.StudentProfileId == leave.StudentProfileId &&
                               c.CreatedReason == CreatedReason.ApprovedLeave24H &&
                               c.SourceSessionId == session.Id, cancellationToken);

            // Luôn tạo MakeupCredit khi đơn xin nghỉ được approve (không còn điều kiện > 24h),
            // nhưng tránh tạo trùng cho cùng 1 session.
            if (!creditExists)
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
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

