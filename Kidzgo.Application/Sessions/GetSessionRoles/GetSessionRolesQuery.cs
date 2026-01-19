using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Sessions.GetSessionRoles;

public sealed class GetSessionRolesQuery : IQuery<GetSessionRolesResponse>
{
    public Guid SessionId { get; init; }
}