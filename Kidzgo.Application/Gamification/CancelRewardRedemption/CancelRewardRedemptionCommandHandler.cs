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
/// Note: Khi cancel, chỉ hoàn lại stars; item quantity được giữ nguyên để dành cho luồng tồn kho sau này.
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

        var totalCostStars = redemption.StarsDeducted;
        if (!totalCostStars.HasValue && redemption.Item == null)
        {
            return Result.Failure<CancelRewardRedemptionResponse>(
                RewardRedemptionErrors.ItemNotFound(redemption.ItemId));
        }

        var now = VietnamTime.UtcNow();
        totalCostStars ??= redemption.Item!.CostStars * redemption.Quantity;

        // Refund stars to student
        var addStarsCommand = new AddStarsCommand
        {
            StudentProfileId = redemption.StudentProfileId,
            Amount = totalCostStars.Value,
            Reason = $"Refund for cancelled redemption: {redemption.ItemName} x{redemption.Quantity}"
        };

        var addStarsResult = await mediator.Send(addStarsCommand, cancellationToken);
        if (addStarsResult.IsFailure)
        {
            return Result.Failure<CancelRewardRedemptionResponse>(addStarsResult.Error);
        }

        // Update redemption status
        redemption.Status = RedemptionStatus.Cancelled;
        redemption.CancelReason = string.IsNullOrWhiteSpace(command.Reason)
            ? null
            : command.Reason.Trim();
        redemption.HandledBy = userContext.UserId;
        redemption.HandledAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new CancelRewardRedemptionResponse
        {
            Id = redemption.Id,
            Status = redemption.Status.ToString(),
            CancelReason = redemption.CancelReason,
            HandledBy = redemption.HandledBy!.Value,
            HandledAt = redemption.HandledAt!.Value
        });
    }
}

