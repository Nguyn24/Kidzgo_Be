using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Authentication.ResetPassword;

public sealed class ResetPasswordCommand : ICommand
{
    public string Token { get; init; } = null!;
    public string NewPassword { get; init; } = null!;
}


