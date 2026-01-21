using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Users.Admin.CreateUser;

public sealed class CreateUserCommand : ICommand<CreateUserCommandResponse>
{
    public string Username { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string Role { get; init; } = null!;
    public Guid? BranchId { get; init; }
    public string? PhoneNumber { get; init; }
}
