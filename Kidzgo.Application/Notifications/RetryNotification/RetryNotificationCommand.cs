using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Notifications.RetryNotification;

public sealed class RetryNotificationCommand : ICommand<RetryNotificationResponse>
{
    public Guid NotificationId { get; init; }
}

