using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Users.Admin.ReactivateUser;

public sealed class ReactivateUserCommand : ICommand<ReactivateUserCommandResponse>
{
    public Guid UserId { get; init; }
}
