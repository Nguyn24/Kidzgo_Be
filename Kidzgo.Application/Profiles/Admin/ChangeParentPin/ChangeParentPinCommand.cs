using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Profiles.Admin.ChangeParentPin;

public sealed class ChangeParentPinCommand : ICommand
{
    public Guid ProfileId { get; init; }
    public string NewPin { get; init; } = null!;
}
