using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MakeupCredits.UseMakeupCredit;

public sealed class UseMakeupCreditCommandHandler(IDbContext context, IUserContext userContext)
    : ICommandHandler<UseMakeupCreditCommand>
{
    public async Task<Result> Handle(UseMakeupCreditCommand command, CancellationToken cancellationToken)
    {
        var credit = await context.MakeupCredits
            .FirstOrDefaultAsync(c => c.Id == command.MakeupCreditId, cancellationToken);

        if (credit is null)
        {
            return Result.Failure(MakeupCreditErrors.NotFound(command.MakeupCreditId));
        }

        if (credit.Status != MakeupCreditStatus.Available)
        {
            return Result.Failure(MakeupCreditErrors.NotAvailable(command.MakeupCreditId));
        }

        if (credit.ExpiresAt.HasValue && credit.ExpiresAt.Value <= DateTime.UtcNow)
        {
            return Result.Failure(MakeupCreditErrors.Expired(command.MakeupCreditId));
        }

        var session = await context.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == command.TargetSessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure(MakeupCreditErrors.NotFound(command.TargetSessionId));
        }

        if (session.ClassId != command.ClassId)
        {
            return Result.Failure(Error.Validation(
                "Session.ClassId",
                "Target session does not belong to the specified class"));
        }

        credit.Status = MakeupCreditStatus.Used;
        credit.UsedSessionId = command.TargetSessionId;

        var allocation = new MakeupAllocation
        {
            Id = Guid.NewGuid(),
            MakeupCreditId = credit.Id,
            TargetSessionId = command.TargetSessionId,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = userContext.UserId
        };

        context.MakeupAllocations.Add(allocation);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}