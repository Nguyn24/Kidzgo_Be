using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Gamification.Errors;

public static class RewardRedemptionErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound(
            "RewardRedemption.NotFound",
            $"Reward redemption {id} not found");

    public static Error ItemNotFound(Guid itemId) =>
        Error.NotFound(
            "RewardRedemption.ItemNotFound",
            $"Reward store item {itemId} not found");

    public static Error ItemNotActive(Guid itemId) =>
        Error.Validation(
            "RewardRedemption.ItemNotActive",
            $"Reward store item {itemId} is not active");

    public static Error InsufficientQuantity(Guid itemId, int available, int requested) =>
        Error.Conflict(
            "RewardRedemption.InsufficientQuantity",
            $"Reward store item {itemId} has insufficient quantity. Available: {available}, Requested: {requested}");

    public static Error InsufficientStars(Guid studentProfileId, int currentBalance, int required) =>
        Error.Conflict(
            "RewardRedemption.InsufficientStars",
            $"Student {studentProfileId} has insufficient stars. Current balance: {currentBalance}, Required: {required}");

    public static Error InvalidStatusTransition(RedemptionStatus currentStatus, RedemptionStatus targetStatus) =>
        Error.Validation(
            "RewardRedemption.InvalidStatusTransition",
            $"Cannot transition from {currentStatus} to {targetStatus}");

    public static Error StudentProfileNotFound(Guid? studentProfileId) =>
        Error.NotFound(
            "RewardRedemption.StudentProfileNotFound",
            $"Student profile {studentProfileId} not found");
}

