using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Authentication.Profiles.ResetParentPin;

public sealed class ResetParentPinCommand : ICommand
{
    public string Token { get; init; } = null!;
    public string NewPin { get; init; } = null!;
}
