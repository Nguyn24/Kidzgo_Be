using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.UpdateMissionRewardRule;

public sealed class UpdateMissionRewardRuleCommandHandler(IDbContext context)
    : ICommandHandler<UpdateMissionRewardRuleCommand, UpdateMissionRewardRuleResponse>
{
    public async Task<Result<UpdateMissionRewardRuleResponse>> Handle(
        UpdateMissionRewardRuleCommand command,
        CancellationToken cancellationToken)
    {
        var rule = await context.MissionRewardRules
            .FirstOrDefaultAsync(rule => rule.Id == command.Id, cancellationToken);

        if (rule is null)
        {
            return Result.Failure<UpdateMissionRewardRuleResponse>(
                MissionRewardRuleErrors.NotFound(command.Id));
        }

        var missionType = command.MissionType ?? rule.MissionType;
        var progressMode = command.ProgressMode ?? rule.ProgressMode;
        var totalRequired = command.TotalRequired ?? rule.TotalRequired;
        var rewardStars = command.RewardStars ?? rule.RewardStars;
        var rewardExp = command.RewardExp ?? rule.RewardExp;

        var validationError = Validate(totalRequired, rewardStars, rewardExp);
        if (validationError is not null)
        {
            return Result.Failure<UpdateMissionRewardRuleResponse>(validationError);
        }

        var isDuplicate = await context.MissionRewardRules
            .AnyAsync(other =>
                other.Id != command.Id &&
                other.MissionType == missionType &&
                other.ProgressMode == progressMode &&
                other.TotalRequired == totalRequired,
                cancellationToken);

        if (isDuplicate)
        {
            return Result.Failure<UpdateMissionRewardRuleResponse>(
                MissionRewardRuleErrors.Duplicate(missionType, progressMode, totalRequired));
        }

        rule.MissionType = missionType;
        rule.ProgressMode = progressMode;
        rule.TotalRequired = totalRequired;
        rule.RewardStars = rewardStars;
        rule.RewardExp = rewardExp;

        if (command.IsActive.HasValue)
        {
            rule.IsActive = command.IsActive.Value;
        }

        rule.UpdatedAt = VietnamTime.UtcNow();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateMissionRewardRuleResponse
        {
            Id = rule.Id,
            MissionType = rule.MissionType.ToString(),
            ProgressMode = rule.ProgressMode.ToString(),
            TotalRequired = rule.TotalRequired,
            RewardStars = rule.RewardStars,
            RewardExp = rule.RewardExp,
            IsActive = rule.IsActive,
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt
        });
    }

    private static Error? Validate(int totalRequired, int rewardStars, int rewardExp)
    {
        if (totalRequired <= 0)
        {
            return MissionRewardRuleErrors.InvalidTotalRequired;
        }

        if (rewardStars < 0 || rewardExp < 0 || rewardStars == 0 && rewardExp == 0)
        {
            return MissionRewardRuleErrors.InvalidReward;
        }

        return null;
    }
}
