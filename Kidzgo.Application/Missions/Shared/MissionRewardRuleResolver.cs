using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Missions.Shared;

public static class MissionRewardRuleResolver
{
    public static async Task<Result<MissionRewardRuleResolution>> ResolveActiveAsync(
        IDbContext context,
        MissionType missionType,
        MissionProgressMode progressMode,
        int? totalRequired,
        CancellationToken cancellationToken)
    {
        if (!totalRequired.HasValue || totalRequired.Value <= 0)
        {
            return Result.Failure<MissionRewardRuleResolution>(
                MissionRewardRuleErrors.InvalidTotalRequired);
        }

        var rule = await context.MissionRewardRules
            .AsNoTracking()
            .Where(rule =>
                rule.IsActive &&
                rule.MissionType == missionType &&
                rule.ProgressMode == progressMode &&
                rule.TotalRequired > 0 &&
                totalRequired.Value % rule.TotalRequired == 0)
            .OrderByDescending(rule => rule.TotalRequired)
            .FirstOrDefaultAsync(cancellationToken);

        if (rule is null)
        {
            return Result.Failure<MissionRewardRuleResolution>(
                MissionRewardRuleErrors.NotConfigured(missionType, progressMode, totalRequired));
        }

        var multiplier = totalRequired.Value / rule.TotalRequired;

        return Result.Success(new MissionRewardRuleResolution
        {
            RuleId = rule.Id,
            TotalRequired = totalRequired.Value,
            RewardStars = rule.RewardStars * multiplier,
            RewardExp = rule.RewardExp * multiplier
        });
    }
}

public sealed class MissionRewardRuleResolution
{
    public Guid RuleId { get; init; }
    public int TotalRequired { get; init; }
    public int RewardStars { get; init; }
    public int RewardExp { get; init; }
}
