using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Notifications.Events;

public sealed record TuitionReminderDomainEvent(
    Guid InvoiceId,
    Guid RecipientUserId,
    Guid? RecipientProfileId,
    decimal Amount,
    DateTime DueDate,
    string? StudentName
) : IDomainEvent;

