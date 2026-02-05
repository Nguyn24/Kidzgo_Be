using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Notifications.Events;

public sealed record MakeupReminderDomainEvent(
    Guid MakeupSessionId,
    Guid RecipientUserId,
    Guid? RecipientProfileId,
    string SessionTitle,
    DateTime SessionStartTime,
    string? ClassName,
    string? Location
) : IDomainEvent;

