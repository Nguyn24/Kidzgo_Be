using Kidzgo.Domain.Notifications;

namespace Kidzgo.API.Requests;

public sealed class BroadcastNotificationRequest
{
    public string Title { get; set; } = null!;
    public string? Content { get; set; }
    public string? Deeplink { get; set; }
    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
    
    // Filters for recipients
    public string? Role { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? StudentProfileId { get; set; }
    public List<Guid>? UserIds { get; set; }
    public List<Guid>? ProfileIds { get; set; }
}

