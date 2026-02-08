using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Notifications.Events;

public sealed record MissionReminderDomainEvent(
    Guid MissionId,
    Guid RecipientUserId,
    Guid? RecipientProfileId,
    string MissionTitle,
    DateTime? DueDate,
    string? ClassName,
    string? StudentName
) : IDomainEvent;

