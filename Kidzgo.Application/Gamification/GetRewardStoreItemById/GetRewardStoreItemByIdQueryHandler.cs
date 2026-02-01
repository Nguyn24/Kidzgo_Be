using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.GetRewardStoreItemById;

/// <summary>
/// UC-223: Xem chi tiết Reward Store Item
/// </summary>
public sealed class GetRewardStoreItemByIdQueryHandler(IDbContext context)
    : IQueryHandler<GetRewardStoreItemByIdQuery, GetRewardStoreItemByIdResponse>
{
    public async Task<Result<GetRewardStoreItemByIdResponse>> Handle(
        GetRewardStoreItemByIdQuery query,
        CancellationToken cancellationToken)
    {
        var item = await context.RewardStoreItems
            .FirstOrDefaultAsync(i => i.Id == query.Id && !i.IsDeleted, cancellationToken);

        if (item == null)
        {
            return Result.Failure<GetRewardStoreItemByIdResponse>(
                RewardStoreErrors.NotFound(query.Id));
        }

        return Result.Success(new GetRewardStoreItemByIdResponse
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            ImageUrl = item.ImageUrl,
            CostStars = item.CostStars,
            Quantity = item.Quantity,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt
        });
    }
}

