using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Gamification.Errors;

public static class XpErrors
{
    public static Error ProfileNotFound(Guid studentProfileId) =>
        Error.NotFound(
            "Xp.ProfileNotFound",
            $"Student profile {studentProfileId} not found");
}

