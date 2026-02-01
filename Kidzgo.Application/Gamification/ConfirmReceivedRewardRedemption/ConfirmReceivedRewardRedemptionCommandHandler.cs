using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.ConfirmReceivedRewardRedemption;

/// <summary>
/// UC-234: Học sinh xác nhận nhận quà (RECEIVED)
/// </summary>
public sealed class ConfirmReceivedRewardRedemptionCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<ConfirmReceivedRewardRedemptionCommand, ConfirmReceivedRewardRedemptionResponse>
{
    public async Task<Result<ConfirmReceivedRewardRedemptionResponse>> Handle(
        ConfirmReceivedRewardRedemptionCommand command,
        CancellationToken cancellationToken)
    {
        // Validate student profile
        if (!userContext.StudentId.HasValue)
        {
            return Result.Failure<ConfirmReceivedRewardRedemptionResponse>(
                RewardRedemptionErrors.StudentProfileNotFound(null));
        }

        var studentProfileId = userContext.StudentId.Value;

        var redemption = await context.RewardRedemptions
            .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

        if (redemption == null)
        {
            return Result.Failure<ConfirmReceivedRewardRedemptionResponse>(
                RewardRedemptionErrors.NotFound(command.Id));
        }

        // Validate student owns this redemption
        if (redemption.StudentProfileId != studentProfileId)
        {
            return Result.Failure<ConfirmReceivedRewardRedemptionResponse>(
                RewardRedemptionErrors.NotFound(command.Id)); // Don't reveal existence
        }

        // Validate status transition: Delivered -> Received
        if (redemption.Status != RedemptionStatus.Delivered)
        {
            return Result.Failure<ConfirmReceivedRewardRedemptionResponse>(
                RewardRedemptionErrors.InvalidStatusTransition(redemption.Status, RedemptionStatus.Received));
        }

        var now = DateTime.UtcNow;
        redemption.Status = RedemptionStatus.Received;
        redemption.ReceivedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new ConfirmReceivedRewardRedemptionResponse
        {
            Id = redemption.Id,
            Status = redemption.Status.ToString(),
            ReceivedAt = redemption.ReceivedAt!.Value
        });
    }
}

