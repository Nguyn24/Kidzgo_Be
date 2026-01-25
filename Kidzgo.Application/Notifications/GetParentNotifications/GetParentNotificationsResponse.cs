using Kidzgo.Application.Notifications.GetNotifications;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Notifications.GetParentNotifications;

public sealed class GetParentNotificationsResponse
{
    public Page<NotificationDto> Notifications { get; init; } = null!;
}

