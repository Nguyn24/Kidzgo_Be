using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using MediatR;
using Kidzgo.Application.Gamification.DeductStars;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Gamification.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.RequestRewardRedemption;

/// <summary>
/// UC-228: Học sinh đổi quà (Request)
/// UC-236: Lưu item_name tại thời điểm đổi
/// UC-237: Trừ Stars khi đổi quà
/// </summary>
public sealed class RequestRewardRedemptionCommandHandler(
    IDbContext context,
    IUserContext userContext,
    ISender mediator
) : ICommandHandler<RequestRewardRedemptionCommand, RequestRewardRedemptionResponse>
{
    public async Task<Result<RequestRewardRedemptionResponse>> Handle(
        RequestRewardRedemptionCommand command,
        CancellationToken cancellationToken)
    {
        // Validate student profile
        if (!userContext.StudentId.HasValue)
        {
            return Result.Failure<RequestRewardRedemptionResponse>(
                RewardRedemptionErrors.StudentProfileNotFound(null));
        }

        var studentProfileId = userContext.StudentId.Value;

        var profile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == studentProfileId && p.ProfileType == ProfileType.Student, cancellationToken);

        if (profile == null)
        {
            return Result.Failure<RequestRewardRedemptionResponse>(
                RewardRedemptionErrors.StudentProfileNotFound(studentProfileId));
        }

        // Validate item exists, is active, and has quantity
        var item = await context.RewardStoreItems
            .FirstOrDefaultAsync(i => i.Id == command.ItemId && !i.IsDeleted, cancellationToken);

        if (item == null)
        {
            return Result.Failure<RequestRewardRedemptionResponse>(
                RewardRedemptionErrors.ItemNotFound(command.ItemId));
        }

        if (!item.IsActive)
        {
            return Result.Failure<RequestRewardRedemptionResponse>(
                RewardRedemptionErrors.ItemNotActive(command.ItemId));
        }

        if (item.Quantity <= 0)
        {
            return Result.Failure<RequestRewardRedemptionResponse>(
                RewardRedemptionErrors.InsufficientQuantity(command.ItemId, item.Quantity, 1));
        }

        // Check student has enough stars
        var currentBalance = await context.StarTransactions
            .Where(t => t.StudentProfileId == studentProfileId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => t.BalanceAfter)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentBalance < item.CostStars)
        {
            return Result.Failure<RequestRewardRedemptionResponse>(
                RewardRedemptionErrors.InsufficientStars(studentProfileId, currentBalance, item.CostStars));
        }

        // UC-237: Deduct stars
        var deductCommand = new DeductStarsCommand
        {
            StudentProfileId = studentProfileId,
            Amount = item.CostStars,
            Reason = $"Redeemed reward: {item.Title}"
        };

        var deductResult = await mediator.Send(deductCommand, cancellationToken);
        if (deductResult.IsFailure)
        {
            return Result.Failure<RequestRewardRedemptionResponse>(deductResult.Error);
        }

        // Decrease item quantity
        item.Quantity -= 1;
        item.UpdatedAt = DateTime.UtcNow;

        // UC-228 & UC-236: Create redemption with item name at redemption time
        var now = DateTime.UtcNow;
        var redemption = new RewardRedemption
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            ItemName = item.Title, // UC-236: Store item name at redemption time
            StudentProfileId = studentProfileId,
            Status = RedemptionStatus.Requested,
            HandledBy = null,
            HandledAt = null,
            DeliveredAt = null,
            ReceivedAt = null,
            CreatedAt = now
        };

        context.RewardRedemptions.Add(redemption);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new RequestRewardRedemptionResponse
        {
            Id = redemption.Id,
            ItemId = redemption.ItemId,
            ItemName = redemption.ItemName,
            StudentProfileId = redemption.StudentProfileId,
            Status = redemption.Status.ToString(),
            StarsDeducted = item.CostStars,
            RemainingStars = deductResult.Value.NewBalance,
            CreatedAt = redemption.CreatedAt
        });
    }
}

