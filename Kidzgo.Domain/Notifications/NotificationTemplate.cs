using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Notifications;

public class NotificationTemplate : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public NotificationChannel Channel { get; set; }
    public string Title { get; set; } = null!;
    public string? Content { get; set; }
    public string? Placeholders { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
