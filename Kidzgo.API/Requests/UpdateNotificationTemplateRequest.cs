using Kidzgo.Domain.Notifications;

namespace Kidzgo.API.Requests;

public sealed class UpdateNotificationTemplateRequest
{
    public NotificationChannel Channel { get; set; }
    public string Title { get; set; } = null!;
    public string? Content { get; set; }
    public string? Placeholders { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
}

