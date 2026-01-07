using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Users.Admin.CreateUser;

public sealed class CreateUserCommand : ICommand<CreateUserCommandResponse>
{
    public string Name { get; init; }
    public string Email { get; init; }
    public string Password { get; init; }
    public UserRole Role { get; init; }
}