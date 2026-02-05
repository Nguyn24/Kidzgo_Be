using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.NotificationTemplates.DeleteNotificationTemplate;

public sealed class DeleteNotificationTemplateCommand : ICommand<DeleteNotificationTemplateResponse>
{
    public Guid Id { get; init; }
}

