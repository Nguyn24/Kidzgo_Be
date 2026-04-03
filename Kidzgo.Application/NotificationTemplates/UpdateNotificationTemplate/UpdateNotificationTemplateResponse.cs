using Kidzgo.Domain.Notifications;

namespace Kidzgo.Application.NotificationTemplates.UpdateNotificationTemplate;

public sealed class UpdateNotificationTemplateResponse
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public NotificationChannel Channel { get; init; }
    public string Title { get; init; } = null!;
    public string? Content { get; init; }
    public string? Placeholders { get; init; }
    public string? Category { get; init; }
    public int UsageCount { get; init; }
    public bool IsActive { get; init; }
    public bool IsDeleted { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

