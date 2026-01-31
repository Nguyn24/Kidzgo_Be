using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.GetMyStarBalance;

public sealed class GetMyStarBalanceQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetMyStarBalanceQuery, GetMyStarBalanceResponse>
{
    public async Task<Result<GetMyStarBalanceResponse>> Handle(
        GetMyStarBalanceQuery query,
        CancellationToken cancellationToken)
    {
        if (!userContext.StudentId.HasValue)
        {
            return Result.Failure<GetMyStarBalanceResponse>(
                StarErrors.ProfileNotFound(Guid.Empty));
        }

        var studentProfileId = userContext.StudentId.Value;

        // Get the latest balance from the most recent transaction
        var balance = await context.StarTransactions
            .Where(t => t.StudentProfileId == studentProfileId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => t.BalanceAfter)
            .FirstOrDefaultAsync(cancellationToken);

        // If no transactions exist, balance is 0
        return Result.Success(new GetMyStarBalanceResponse
        {
            StudentProfileId = studentProfileId,
            Balance = balance
        });
    }
}

