using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Notifications.Events;

public sealed record SessionReminderDomainEvent(
    Guid SessionId,
    Guid RecipientUserId,
    Guid? RecipientProfileId,
    string SessionTitle,
    DateTime SessionStartTime,
    string? ClassName,
    string? Location,
    string? StudentName,
    string? ClassroomName
) : IDomainEvent;

