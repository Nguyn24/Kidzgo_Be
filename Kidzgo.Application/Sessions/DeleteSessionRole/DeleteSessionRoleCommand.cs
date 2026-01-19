using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Sessions.DeleteSessionRole;

public sealed class DeleteSessionRoleCommand : ICommand
{
    public Guid SessionRoleId { get; init; }
}