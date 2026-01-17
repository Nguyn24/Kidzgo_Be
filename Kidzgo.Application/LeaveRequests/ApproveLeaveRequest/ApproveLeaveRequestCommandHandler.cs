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

        // Create makeup credit if none exists for this leave/session date
        bool creditExists = await context.MakeupCredits
            .AnyAsync(c => c.StudentProfileId == leave.StudentProfileId &&
                           c.CreatedReason == CreatedReason.ApprovedLeave24H &&
                           c.SourceSessionId == Guid.Empty, cancellationToken);

        if (!creditExists && leave.NoticeHours.GetValueOrDefault() >= 24)
        {
            var credit = new MakeupCredit
            {
                Id = Guid.NewGuid(),
                StudentProfileId = leave.StudentProfileId,
                SourceSessionId = Guid.Empty,
                Status = MakeupCreditStatus.Available,
                CreatedReason = CreatedReason.ApprovedLeave24H,
                ExpiresAt = null,
                CreatedAt = DateTime.UtcNow
            };
            context.MakeupCredits.Add(credit);
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

