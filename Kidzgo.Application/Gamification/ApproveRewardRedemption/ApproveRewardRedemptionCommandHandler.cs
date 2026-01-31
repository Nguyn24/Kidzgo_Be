using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.ApproveRewardRedemption;

/// <summary>
/// UC-231: Staff duyệt Reward Redemption (APPROVED)
/// </summary>
public sealed class ApproveRewardRedemptionCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<ApproveRewardRedemptionCommand, ApproveRewardRedemptionResponse>
{
    public async Task<Result<ApproveRewardRedemptionResponse>> Handle(
        ApproveRewardRedemptionCommand command,
        CancellationToken cancellationToken)
    {
        var redemption = await context.RewardRedemptions
            .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

        if (redemption == null)
        {
            return Result.Failure<ApproveRewardRedemptionResponse>(
                RewardRedemptionErrors.NotFound(command.Id));
        }

        // Validate status transition: Requested -> Approved
        if (redemption.Status != RedemptionStatus.Requested)
        {
            return Result.Failure<ApproveRewardRedemptionResponse>(
                RewardRedemptionErrors.InvalidStatusTransition(redemption.Status, RedemptionStatus.Approved));
        }

        var now = DateTime.UtcNow;
        redemption.Status = RedemptionStatus.Approved;
        redemption.HandledBy = userContext.UserId;
        redemption.HandledAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new ApproveRewardRedemptionResponse
        {
            Id = redemption.Id,
            Status = redemption.Status.ToString(),
            HandledBy = redemption.HandledBy!.Value,
            HandledAt = redemption.HandledAt!.Value
        });
    }
}

