using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PauseEnrollmentRequests.CancelPauseEnrollmentRequest;

public sealed class CancelPauseEnrollmentRequestCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<CancelPauseEnrollmentRequestCommand>
{
    public async Task<Result> Handle(CancelPauseEnrollmentRequestCommand request, CancellationToken cancellationToken)
    {
        var pauseRequest = await context.PauseEnrollmentRequests
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (pauseRequest is null)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.NotFound(request.Id));
        }

        if (pauseRequest.Status == PauseEnrollmentRequestStatus.Cancelled)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.AlreadyCancelled);
        }

        if (pauseRequest.Status == PauseEnrollmentRequestStatus.Approved)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.AlreadyApproved);
        }

        if (pauseRequest.Status == PauseEnrollmentRequestStatus.Rejected)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.AlreadyRejected);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (today >= pauseRequest.PauseFrom)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.CancelWindowExpired(pauseRequest.PauseFrom));
        }

        pauseRequest.Status = PauseEnrollmentRequestStatus.Cancelled;
        pauseRequest.CancelledAt = DateTime.UtcNow;
        pauseRequest.CancelledBy = userContext.UserId;

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
