using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.UpdateRewardStoreItem;

/// <summary>
/// UC-224: Cập nhật Reward Store Item
/// UC-226: Thiết lập cost_stars cho Item
/// UC-227: Quản lý quantity của Item
/// </summary>
public sealed class UpdateRewardStoreItemCommandHandler(IDbContext context)
    : ICommandHandler<UpdateRewardStoreItemCommand, UpdateRewardStoreItemResponse>
{
    public async Task<Result<UpdateRewardStoreItemResponse>> Handle(
        UpdateRewardStoreItemCommand command,
        CancellationToken cancellationToken)
    {
        var item = await context.RewardStoreItems
            .FirstOrDefaultAsync(i => i.Id == command.Id && !i.IsDeleted, cancellationToken);

        if (item == null)
        {
            return Result.Failure<UpdateRewardStoreItemResponse>(
                RewardStoreErrors.NotFound(command.Id));
        }

        if (command.Title != null)
        {
            item.Title = string.IsNullOrWhiteSpace(command.Title) ? item.Title : command.Title.Trim();
        }

        if (command.Description != null)
        {
            item.Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim();
        }

        if (command.ImageUrl != null)
        {
            item.ImageUrl = string.IsNullOrWhiteSpace(command.ImageUrl) ? null : command.ImageUrl.Trim();
        }

        if (command.CostStars.HasValue)
        {
            if (command.CostStars.Value <= 0)
            {
                return Result.Failure<UpdateRewardStoreItemResponse>(
                    RewardStoreErrors.InvalidCostStars);
            }
            item.CostStars = command.CostStars.Value;
        }

        if (command.Quantity.HasValue)
        {
            if (command.Quantity.Value < 0)
            {
                return Result.Failure<UpdateRewardStoreItemResponse>(
                    RewardStoreErrors.InvalidQuantity);
            }
            item.Quantity = command.Quantity.Value;
        }

        if (command.IsActive.HasValue)
        {
            item.IsActive = command.IsActive.Value;
        }

        item.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateRewardStoreItemResponse
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

