using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Notifications;

namespace Kidzgo.Application.NotificationTemplates.UpdateNotificationTemplate;

public sealed class UpdateNotificationTemplateCommand : ICommand<UpdateNotificationTemplateResponse>
{
    public Guid Id { get; init; }
    public NotificationChannel Channel { get; init; }
    public string Title { get; init; } = null!;
    public string? Content { get; init; }
    public string? Placeholders { get; init; }
    public bool IsActive { get; init; }
}

