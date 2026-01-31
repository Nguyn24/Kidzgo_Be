using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.DeductStars;

public sealed class DeductStarsCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<DeductStarsCommand, DeductStarsResponse>
{
    public async Task<Result<DeductStarsResponse>> Handle(
        DeductStarsCommand command,
        CancellationToken cancellationToken)
    {
        // Validate student profile exists
        var profile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId && p.ProfileType == Domain.Users.ProfileType.Student, cancellationToken);

        if (profile == null)
        {
            return Result.Failure<DeductStarsResponse>(
                Domain.Gamification.Errors.StarErrors.ProfileNotFound(command.StudentProfileId));
        }

        // Calculate current balance
        var currentBalance = await context.StarTransactions
            .Where(t => t.StudentProfileId == command.StudentProfileId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => t.BalanceAfter)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentBalance < command.Amount)
        {
            return Result.Failure<DeductStarsResponse>(
                Domain.Gamification.Errors.StarErrors.InsufficientStars(command.StudentProfileId, currentBalance, command.Amount));
        }

        var newBalance = currentBalance - command.Amount;

        // Create transaction
        var transaction = new StarTransaction
        {
            Id = Guid.NewGuid(),
            StudentProfileId = command.StudentProfileId,
            Amount = -command.Amount, // Negative amount for deduction
            Reason = command.Reason,
            SourceType = StarSourceType.Adjustment,
            SourceId = null,
            BalanceAfter = newBalance,
            CreatedBy = userContext.UserId,
            CreatedAt = DateTime.UtcNow
        };

        context.StarTransactions.Add(transaction);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new DeductStarsResponse
        {
            StudentProfileId = command.StudentProfileId,
            Amount = command.Amount,
            NewBalance = newBalance,
            TransactionId = transaction.Id
        });
    }
}

