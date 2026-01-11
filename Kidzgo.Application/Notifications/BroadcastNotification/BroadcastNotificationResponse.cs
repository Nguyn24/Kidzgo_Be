namespace Kidzgo.Application.Notifications.BroadcastNotification;

public sealed class BroadcastNotificationResponse
{
    public int CreatedCount { get; init; }
    public List<Guid> CreatedNotificationIds { get; init; } = new();
}

