using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;

namespace Kidzgo.Domain.Gamification.Errors;

public static class MissionRewardRuleErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "MissionRewardRule.NotFound",
        $"Mission reward rule with Id = '{id}' was not found");

    public static Error Duplicate(MissionType missionType, MissionProgressMode progressMode, int totalRequired) =>
        Error.Conflict(
            "MissionRewardRule.Duplicate",
            $"Mission reward rule for {missionType}/{progressMode} with totalRequired = {totalRequired} already exists");

    public static Error NotConfigured(MissionType missionType, MissionProgressMode progressMode, int? totalRequired) =>
        Error.Validation(
            "MissionRewardRule.NotConfigured",
            $"No active mission reward rule can resolve {missionType}/{progressMode} with totalRequired = {totalRequired}");

    public static readonly Error InvalidTotalRequired = Error.Validation(
        "MissionRewardRule.InvalidTotalRequired",
        "TotalRequired must be greater than 0");

    public static readonly Error InvalidReward = Error.Validation(
        "MissionRewardRule.InvalidReward",
        "RewardStars and RewardExp cannot be negative, and at least one reward value must be greater than 0");
}
