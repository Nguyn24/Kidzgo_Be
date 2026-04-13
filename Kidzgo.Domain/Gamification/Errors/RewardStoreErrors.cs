using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Gamification.Errors;

public static class RewardStoreErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound(
            "RewardStore.NotFound",
            $"Reward store item {id} not found");

    public static Error InvalidCostStars =>
        Error.Validation(
            "RewardStore.InvalidCostStars",
            "Cost stars must be greater than 0");
}

