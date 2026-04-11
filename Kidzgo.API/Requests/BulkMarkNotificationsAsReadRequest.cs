namespace Kidzgo.API.Requests;

public sealed class BulkMarkNotificationsAsReadRequest
{
    public List<Guid> NotificationIds { get; set; } = new();
}
