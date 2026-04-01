using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Authentication.Login;

public sealed class LoginByPhoneNumberCommand() : ICommand<TokenResponse>
{
    public string PhoneNumber { get; init; } = null!;
}
