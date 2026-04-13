using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.CreateMissionRewardRule;

public sealed class CreateMissionRewardRuleCommandHandler(IDbContext context)
    : ICommandHandler<CreateMissionRewardRuleCommand, CreateMissionRewardRuleResponse>
{
    public async Task<Result<CreateMissionRewardRuleResponse>> Handle(
        CreateMissionRewardRuleCommand command,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(command.TotalRequired, command.RewardStars, command.RewardExp);
        if (validationError is not null)
        {
            return Result.Failure<CreateMissionRewardRuleResponse>(validationError);
        }

        var isDuplicate = await context.MissionRewardRules
            .AnyAsync(rule =>
                rule.MissionType == command.MissionType &&
                rule.ProgressMode == command.ProgressMode &&
                rule.TotalRequired == command.TotalRequired,
                cancellationToken);

        if (isDuplicate)
        {
            return Result.Failure<CreateMissionRewardRuleResponse>(
                MissionRewardRuleErrors.Duplicate(
                    command.MissionType,
                    command.ProgressMode,
                    command.TotalRequired));
        }

        var now = VietnamTime.UtcNow();
        var rule = new MissionRewardRule
        {
            Id = Guid.NewGuid(),
            MissionType = command.MissionType,
            ProgressMode = command.ProgressMode,
            TotalRequired = command.TotalRequired,
            RewardStars = command.RewardStars,
            RewardExp = command.RewardExp,
            IsActive = command.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.MissionRewardRules.Add(rule);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateMissionRewardRuleResponse
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
