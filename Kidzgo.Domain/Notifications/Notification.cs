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
    public DateTime? ReadAt { get; set; }
    public string? TemplateId { get; set; }
    public Guid? NotificationTemplateId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Push notification metadata
    public string? TargetRole { get; set; } // Role nhận notification: Parent, Student, Teacher, Staff
    public string? Kind { get; set; } // Loại notification: report, attendance, homework, message, payment
    public string? Priority { get; set; } // Độ ưu tiên: high, normal, low
    public string? SenderRole { get; set; } // Role người gửi: System, Teacher, Admin, Staff
    public string? SenderName { get; set; } // Tên người gửi/hệ thống
    public Guid? ScopeBranchId { get; set; }
    public Guid? ScopeClassId { get; set; }
    public Guid? ScopeStudentProfileId { get; set; }

    // Navigation properties
    public User RecipientUser { get; set; } = null!;
    public Profile? RecipientProfile { get; set; }
    public NotificationTemplate? NotificationTemplate { get; set; }
}
