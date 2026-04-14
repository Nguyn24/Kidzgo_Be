using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Sessions.UpdateSessionColor;

public sealed class UpdateSessionColorCommand : ICommand
{
    public Guid SessionId { get; init; }
    public string Color { get; init; } = string.Empty;
}
