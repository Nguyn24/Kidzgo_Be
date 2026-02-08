using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Notifications.Events;

public sealed record HomeworkReminderDomainEvent(
    Guid HomeworkId,
    Guid RecipientUserId,
    Guid? RecipientProfileId,
    string HomeworkTitle,
    DateTime? DueDate,
    string? ClassName,
    string? StudentName
) : IDomainEvent;

