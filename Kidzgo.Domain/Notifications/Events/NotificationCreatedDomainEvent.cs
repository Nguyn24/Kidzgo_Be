using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Notifications.Events;

public sealed record NotificationCreatedDomainEvent(
    Guid NotificationId,
    NotificationChannel Channel
) : IDomainEvent;

