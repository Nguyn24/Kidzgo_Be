using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.CreateRewardStoreItem;

/// <summary>
/// UC-221: Tạo Reward Store Item
/// </summary>
public sealed class CreateRewardStoreItemCommandHandler(
    IDbContext context)
    : ICommandHandler<CreateRewardStoreItemCommand, CreateRewardStoreItemResponse>
{
    public async Task<Result<CreateRewardStoreItemResponse>> Handle(
        CreateRewardStoreItemCommand command,
        CancellationToken cancellationToken)
    {
        if (command.CostStars <= 0)
        {
            return Result.Failure<CreateRewardStoreItemResponse>(
                RewardStoreErrors.InvalidCostStars);
        }

        if (command.Quantity < 0)
        {
            return Result.Failure<CreateRewardStoreItemResponse>(
                RewardStoreErrors.InvalidQuantity);
        }

        var now = DateTime.UtcNow;
        var item = new RewardStoreItem
        {
            Id = Guid.NewGuid(),
            Title = command.Title.Trim(),
            Description = command.Description?.Trim(),
            ImageUrl = command.ImageUrl?.Trim(),
            CostStars = command.CostStars,
            Quantity = command.Quantity,
            IsActive = command.IsActive,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.RewardStoreItems.Add(item);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateRewardStoreItemResponse
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

