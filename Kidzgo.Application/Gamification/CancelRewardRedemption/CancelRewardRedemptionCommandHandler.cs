using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using MediatR;
using Kidzgo.Application.Gamification.AddStars;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.CancelRewardRedemption;

/// <summary>
/// UC-232: Staff từ chối Reward Redemption (CANCELLED)
/// Note: Khi cancel, cần hoàn lại stars và tăng lại quantity của item
/// </summary>
public sealed class CancelRewardRedemptionCommandHandler(
    IDbContext context,
    IUserContext userContext,
    ISender mediator
) : ICommandHandler<CancelRewardRedemptionCommand, CancelRewardRedemptionResponse>
{
    public async Task<Result<CancelRewardRedemptionResponse>> Handle(
        CancelRewardRedemptionCommand command,
        CancellationToken cancellationToken)
    {
        var redemption = await context.RewardRedemptions
            .Include(r => r.Item)
            .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

        if (redemption == null)
        {
            return Result.Failure<CancelRewardRedemptionResponse>(
                RewardRedemptionErrors.NotFound(command.Id));
        }

        // Validate status transition: Can cancel from Requested or Approved
        if (redemption.Status != RedemptionStatus.Requested && redemption.Status != RedemptionStatus.Approved)
        {
            return Result.Failure<CancelRewardRedemptionResponse>(
                RewardRedemptionErrors.InvalidStatusTransition(redemption.Status, RedemptionStatus.Cancelled));
        }

        // Get item to restore quantity and get cost stars
        var item = redemption.Item;
        if (item == null)
        {
            return Result.Failure<CancelRewardRedemptionResponse>(
                RewardRedemptionErrors.ItemNotFound(redemption.ItemId));
        }

        var now = DateTime.UtcNow;

        // Restore item quantity
        item.Quantity += 1;
        item.UpdatedAt = now;

        // Refund stars to student
        var addStarsCommand = new AddStarsCommand
        {
            StudentProfileId = redemption.StudentProfileId,
            Amount = item.CostStars, // Refund the cost
            Reason = $"Refund for cancelled redemption: {redemption.ItemName}"
        };

        var addStarsResult = await mediator.Send(addStarsCommand, cancellationToken);
        if (addStarsResult.IsFailure)
        {
            return Result.Failure<CancelRewardRedemptionResponse>(addStarsResult.Error);
        }

        // Update redemption status
        redemption.Status = RedemptionStatus.Cancelled;
        redemption.HandledBy = userContext.UserId;
        redemption.HandledAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new CancelRewardRedemptionResponse
        {
            Id = redemption.Id,
            Status = redemption.Status.ToString(),
            HandledBy = redemption.HandledBy!.Value,
            HandledAt = redemption.HandledAt!.Value
        });
    }
}

