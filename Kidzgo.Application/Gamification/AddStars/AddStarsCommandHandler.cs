using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.AddStars;

public sealed class AddStarsCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<AddStarsCommand, AddStarsResponse>
{
    public async Task<Result<AddStarsResponse>> Handle(
        AddStarsCommand command,
        CancellationToken cancellationToken)
    {
        // Validate student profile exists
        var profile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId && p.ProfileType == Domain.Users.ProfileType.Student, cancellationToken);

        if (profile == null)
        {
            return Result.Failure<AddStarsResponse>(
                Domain.Gamification.Errors.StarErrors.ProfileNotFound(command.StudentProfileId));
        }

        // Calculate current balance
        var currentBalance = await context.StarTransactions
            .Where(t => t.StudentProfileId == command.StudentProfileId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => t.BalanceAfter)
            .FirstOrDefaultAsync(cancellationToken);

        var newBalance = currentBalance + command.Amount;

        // Create transaction
        var transaction = new StarTransaction
        {
            Id = Guid.NewGuid(),
            StudentProfileId = command.StudentProfileId,
            Amount = command.Amount,
            Reason = command.Reason,
            SourceType = StarSourceType.Manual,
            SourceId = null,
            BalanceAfter = newBalance,
            CreatedBy = userContext.UserId,
            CreatedAt = DateTime.UtcNow
        };

        context.StarTransactions.Add(transaction);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new AddStarsResponse
        {
            StudentProfileId = command.StudentProfileId,
            Amount = command.Amount,
            NewBalance = newBalance,
            TransactionId = transaction.Id
        });
    }
}

