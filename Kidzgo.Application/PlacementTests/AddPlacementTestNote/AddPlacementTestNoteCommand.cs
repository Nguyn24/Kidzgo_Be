using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.PlacementTests.AddPlacementTestNote;

public sealed class AddPlacementTestNoteCommand : ICommand<AddPlacementTestNoteResponse>
{
    public Guid PlacementTestId { get; init; }
    public string Note { get; init; } = null!;
}

