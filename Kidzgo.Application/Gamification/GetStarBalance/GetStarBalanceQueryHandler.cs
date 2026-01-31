using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.GetStarBalance;

public sealed class GetStarBalanceQueryHandler(
    IDbContext context
) : IQueryHandler<GetStarBalanceQuery, GetStarBalanceResponse>
{
    public async Task<Result<GetStarBalanceResponse>> Handle(
        GetStarBalanceQuery query,
        CancellationToken cancellationToken)
    {
        // Get the latest balance from the most recent transaction
        var balance = await context.StarTransactions
            .Where(t => t.StudentProfileId == query.StudentProfileId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => t.BalanceAfter)
            .FirstOrDefaultAsync(cancellationToken);

        // If no transactions exist, balance is 0
        return Result.Success(new GetStarBalanceResponse
        {
            StudentProfileId = query.StudentProfileId,
            Balance = balance
        });
    }
}

