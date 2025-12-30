using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Authentication.ForgetPassword;

public sealed class ForgetPasswordCommand : ICommand
{
    public string Email { get; init; } = null!;
}


