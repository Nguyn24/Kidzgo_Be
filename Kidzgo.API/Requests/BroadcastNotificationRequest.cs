using Kidzgo.Domain.Notifications;

namespace Kidzgo.API.Requests;

public sealed class BroadcastNotificationRequest
{
    public string Title { get; set; } = null!;
    public string? Content { get; set; }
    public string? Deeplink { get; set; }
    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
    public string? Kind { get; set; }
    public string? Priority { get; set; }
    public string? SenderRole { get; set; }
    public string? SenderName { get; set; }
    
    // Filters for recipients
    public string? Role { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? StudentProfileId { get; set; }
    public List<Guid>? UserIds { get; set; }
    public List<Guid>? ProfileIds { get; set; }
}

