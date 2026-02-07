using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Notifications.Events;

public sealed record MediaReminderDomainEvent(
    Guid MediaId,
    Guid RecipientUserId,
    Guid? RecipientProfileId,
    string MediaTitle,
    string? MediaType,
    string? ClassName,
    string? StudentName
) : IDomainEvent;

