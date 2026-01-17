using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LeaveRequests.RejectLeaveRequest;

public sealed class RejectLeaveRequestCommandHandler(IDbContext context)
    : ICommandHandler<RejectLeaveRequestCommand>
{
    public async Task<Result> Handle(RejectLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leave = await context.LeaveRequests
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (leave is null)
        {
            return Result.Failure(LeaveRequestErrors.NotFound(request.Id));
        }

        if (leave.Status == LeaveRequestStatus.Rejected)
        {
            return Result.Failure(LeaveRequestErrors.AlreadyRejected);
        }

        leave.Status = LeaveRequestStatus.Rejected;
        leave.ApprovedAt = null;
        leave.ApprovedBy = null;

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

