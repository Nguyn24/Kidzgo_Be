using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PauseEnrollmentRequests.UpdatePauseEnrollmentOutcome;

public sealed class UpdatePauseEnrollmentOutcomeCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<UpdatePauseEnrollmentOutcomeCommand>
{
    public async Task<Result> Handle(UpdatePauseEnrollmentOutcomeCommand request, CancellationToken cancellationToken)
    {
        var pauseRequest = await context.PauseEnrollmentRequests
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (pauseRequest is null)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.NotFound(request.Id));
        }

        if (pauseRequest.Status != PauseEnrollmentRequestStatus.Approved)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.OutcomeNotAllowed);
        }

        pauseRequest.Outcome = request.Outcome;
        pauseRequest.OutcomeNote = request.OutcomeNote;
        pauseRequest.OutcomeBy = userContext.UserId;
        pauseRequest.OutcomeAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
