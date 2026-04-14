using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Classes.UpdateClassColor;

public sealed class UpdateClassColorCommand : ICommand
{
    public Guid ClassId { get; init; }
    public string Color { get; init; } = string.Empty;
}
