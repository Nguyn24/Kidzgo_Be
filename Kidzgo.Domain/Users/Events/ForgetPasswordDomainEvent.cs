using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Users.Events;

public sealed record ForgetPasswordDomainEvent(Guid UserId) : IDomainEvent;





