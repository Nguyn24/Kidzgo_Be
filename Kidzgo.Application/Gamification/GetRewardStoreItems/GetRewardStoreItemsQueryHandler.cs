using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.GetRewardStoreItems;

/// <summary>
/// UC-222: Xem danh sách Reward Store Items
/// </summary>
public sealed class GetRewardStoreItemsQueryHandler(IDbContext context)
    : IQueryHandler<GetRewardStoreItemsQuery, GetRewardStoreItemsResponse>
{
    public async Task<Result<GetRewardStoreItemsResponse>> Handle(
        GetRewardStoreItemsQuery query,
        CancellationToken cancellationToken)
    {
        var itemsQuery = context.RewardStoreItems
            .Where(i => !i.IsDeleted)
            .AsQueryable();

        if (query.IsActive.HasValue)
        {
            itemsQuery = itemsQuery.Where(i => i.IsActive == query.IsActive.Value);
        }

        var totalCount = await itemsQuery.CountAsync(cancellationToken);

        var items = await itemsQuery
            .OrderByDescending(i => i.CreatedAt)
            .ApplyPagination(query.Page, query.PageSize)
            .Select(i => new RewardStoreItemDto
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description,
                ImageUrl = i.ImageUrl,
                CostStars = i.CostStars,
                Quantity = i.Quantity,
                IsActive = i.IsActive,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var page = new Page<RewardStoreItemDto>(items, totalCount, query.Page, query.PageSize);

        return Result.Success(new GetRewardStoreItemsResponse
        {
            Items = page
        });
    }
}

