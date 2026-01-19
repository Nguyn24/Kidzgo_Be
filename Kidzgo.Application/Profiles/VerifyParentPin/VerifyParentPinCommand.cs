using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Profiles.VerifyParentPin;

public sealed class VerifyParentPinCommand : ICommand
{
    public Guid ProfileId { get; init; }
    public string Pin { get; init; } = null!;
}





















