using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Notifications;

public class Notification : Entity
{
    public Guid Id { get; set; }
    public Guid RecipientUserId { get; set; }
    public Guid? RecipientProfileId { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Title { get; set; } = null!;
    public string? Content { get; set; }
    public string? Deeplink { get; set; }
    public NotificationStatus Status { get; set; }
    public DateTime? SentAt { get; set; }
    public string? TemplateId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User RecipientUser { get; set; } = null!;
    public Profile? RecipientProfile { get; set; }
}
