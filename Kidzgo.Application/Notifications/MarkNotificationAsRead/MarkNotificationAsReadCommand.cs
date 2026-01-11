using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Notifications.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadCommand : ICommand<MarkNotificationAsReadResponse>
{
    public Guid NotificationId { get; init; }
}

