using Kidzgo.Domain.Common;
using Kidzgo.Domain.Notifications;

namespace Kidzgo.Application.NotificationTemplates.GetAllNotificationTemplates;

public sealed class GetAllNotificationTemplatesResponse
{
    public Page<NotificationTemplateDto> Templates { get; init; } = null!;
}

public sealed class NotificationTemplateDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public NotificationChannel Channel { get; init; }
    public string Title { get; init; } = null!;
    public string? Content { get; init; }
    public List<string> Placeholders { get; init; } = new();
    public string? PlaceholdersRaw { get; init; }
    public bool IsActive { get; init; }
    public bool IsDeleted { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string Status => IsActive ? "Active" : "Inactive";
    public string? Category { get; init; }
    public int? UsageCount { get; init; }
}

