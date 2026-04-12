using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Notifications.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadCommand : ICommand<MarkNotificationAsReadResponse>
{
    public IReadOnlyList<Guid> NotificationIds { get; init; } = Array.Empty<Guid>();
}

