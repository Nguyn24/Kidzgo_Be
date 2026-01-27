using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.PlacementTests.CancelPlacementTest;

public sealed class CancelPlacementTestCommand : ICommand<CancelPlacementTestResponse>
{
    public Guid PlacementTestId { get; init; }
    public string? Reason { get; init; }
}

