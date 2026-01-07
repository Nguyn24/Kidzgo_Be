using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Users.Events;

public sealed record ParentPinResetRequestDomainEvent(Guid ProfileId, Guid UserId) : IDomainEvent;

