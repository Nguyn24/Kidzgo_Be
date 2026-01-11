namespace Kidzgo.Application.Notifications.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadResponse
{
    public Guid Id { get; init; }
    public DateTime ReadAt { get; init; }
}

