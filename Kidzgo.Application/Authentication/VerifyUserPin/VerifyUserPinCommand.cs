using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Authentication.VerifyUserPin;

public sealed class VerifyUserPinCommand : ICommand
{
    public string Pin { get; init; } = null!;
}












