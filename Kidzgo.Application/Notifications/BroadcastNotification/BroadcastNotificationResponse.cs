namespace Kidzgo.Application.Notifications.BroadcastNotification;

public sealed class BroadcastNotificationResponse
{
    public Guid? Id { get; init; }
    public Guid? CampaignId { get; init; }
    public DateTime CreatedAt { get; init; }
    public int CreatedCount { get; init; }
    public int DeliveredCount { get; init; }
    public List<Guid> CreatedNotificationIds { get; init; } = new();
}

