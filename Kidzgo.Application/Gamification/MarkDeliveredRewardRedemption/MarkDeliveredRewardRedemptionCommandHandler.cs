using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.MarkDeliveredRewardRedemption;

/// <summary>
/// UC-233: Staff trao quà (DELIVERED)
/// </summary>
public sealed class MarkDeliveredRewardRedemptionCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<MarkDeliveredRewardRedemptionCommand, MarkDeliveredRewardRedemptionResponse>
{
    public async Task<Result<MarkDeliveredRewardRedemptionResponse>> Handle(
        MarkDeliveredRewardRedemptionCommand command,
        CancellationToken cancellationToken)
    {
        var redemption = await context.RewardRedemptions
            .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

        if (redemption == null)
        {
            return Result.Failure<MarkDeliveredRewardRedemptionResponse>(
                RewardRedemptionErrors.NotFound(command.Id));
        }

        // Validate status transition: Approved -> Delivered
        if (redemption.Status != RedemptionStatus.Approved)
        {
            return Result.Failure<MarkDeliveredRewardRedemptionResponse>(
                RewardRedemptionErrors.InvalidStatusTransition(redemption.Status, RedemptionStatus.Delivered));
        }

        var now = DateTime.UtcNow;
        redemption.Status = RedemptionStatus.Delivered;
        redemption.DeliveredAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new MarkDeliveredRewardRedemptionResponse
        {
            Id = redemption.Id,
            Status = redemption.Status.ToString(),
            DeliveredAt = redemption.DeliveredAt!.Value
        });
    }
}

