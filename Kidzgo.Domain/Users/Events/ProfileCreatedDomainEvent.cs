using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Users.Events;

public sealed record ProfileCreatedDomainEvent(
    Guid ProfileId,
    Guid UserId,
    string ProfileType,
    string DisplayName,
    string FullName,
    string Gender,
    string Birthday,
    string ZaloId,
    string Email,
    string Phone,
    string CreatedAt
) : IDomainEvent;

