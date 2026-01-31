using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Gamification.Errors;

public static class StarErrors
{
    public static Error InsufficientStars(Guid studentProfileId, int currentBalance, int requestedAmount) =>
        Error.Validation(
            "Star.InsufficientBalance",
            $"Student {studentProfileId} has insufficient stars. Current balance: {currentBalance}, Requested: {requestedAmount}");

    public static Error ProfileNotFound(Guid studentProfileId) =>
        Error.NotFound(
            "Star.ProfileNotFound",
            $"Student profile {studentProfileId} not found");
}

