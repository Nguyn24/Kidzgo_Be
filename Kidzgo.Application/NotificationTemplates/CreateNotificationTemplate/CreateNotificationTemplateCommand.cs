using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Notifications;

namespace Kidzgo.Application.NotificationTemplates.CreateNotificationTemplate;

public sealed class CreateNotificationTemplateCommand : ICommand<CreateNotificationTemplateResponse>
{
    public string Code { get; init; } = null!;
    public NotificationChannel Channel { get; init; }
    public string Title { get; init; } = null!;
    public string? Content { get; init; }
    public string? Placeholders { get; init; }
    public bool IsActive { get; init; } = true;
}

