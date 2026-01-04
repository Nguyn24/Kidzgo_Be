using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Authentication.ChangePin;

public sealed class ChangeUserPinCommand : ICommand
{
    public string CurrentPin { get; init; } = null!;
    public string NewPin { get; init; } = null!;
}







