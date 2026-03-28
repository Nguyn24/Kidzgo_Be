namespace Kidzgo.Application.Users.Shared;

public static class UserPresenceHelper
{
    public static readonly TimeSpan HeartbeatUpdateInterval = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan OnlineThreshold = TimeSpan.FromMinutes(2);

    public static bool IsOnline(DateTime? lastSeenAt, DateTime utcNow)
    {
        return lastSeenAt.HasValue && utcNow - lastSeenAt.Value <= OnlineThreshold;
    }

    public static long? GetOfflineDurationSeconds(DateTime? lastSeenAt, DateTime utcNow)
    {
        if (!lastSeenAt.HasValue)
        {
            return null;
        }

        if (IsOnline(lastSeenAt, utcNow))
        {
            return null;
        }

        return Math.Max(0L, (long)(utcNow - lastSeenAt.Value).TotalSeconds);
    }
}
