namespace Kidzgo.Application.Notifications.RetryNotification;

public sealed class RetryNotificationResponse
{
    public Guid NotificationId { get; init; }
    public string Status { get; init; } = null!;
    public DateTime RetriedAt { get; init; }
}

