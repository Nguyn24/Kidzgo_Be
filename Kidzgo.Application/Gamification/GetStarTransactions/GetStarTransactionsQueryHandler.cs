using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.GetStarTransactions;

public sealed class GetStarTransactionsQueryHandler(
    IDbContext context
) : IQueryHandler<GetStarTransactionsQuery, GetStarTransactionsResponse>
{
    public async Task<Result<GetStarTransactionsResponse>> Handle(
        GetStarTransactionsQuery query,
        CancellationToken cancellationToken)
    {
        var transactionsQuery = context.StarTransactions
            .Include(t => t.CreatedByUser)
            .Where(t => t.StudentProfileId == query.StudentProfileId)
            .AsQueryable();

        var totalCount = await transactionsQuery.CountAsync(cancellationToken);

        var transactions = await transactionsQuery
            .OrderByDescending(t => t.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => new StarTransactionDto
            {
                Id = t.Id,
                StudentProfileId = t.StudentProfileId,
                Amount = t.Amount,
                Reason = t.Reason,
                SourceType = t.SourceType.ToString(),
                SourceId = t.SourceId,
                BalanceAfter = t.BalanceAfter,
                CreatedBy = t.CreatedBy,
                CreatedByName = t.CreatedByUser != null
                    ? t.CreatedByUser.Name ?? t.CreatedByUser.Email
                    : null,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetStarTransactionsResponse
        {
            Transactions = transactions,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }
}

