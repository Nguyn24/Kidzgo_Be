using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Authentication.Login;

public sealed class LoginCommand() : ICommand<TokenResponse>
{
    public string Email { get; init; }
    public string Password { get; init; }
}