namespace Kidzgo.Application.NotificationTemplates.DeleteNotificationTemplate;

public sealed class DeleteNotificationTemplateResponse
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string Title { get; init; } = null!;
    public bool IsDeleted { get; init; }
    public DateTime UpdatedAt { get; init; }
}

