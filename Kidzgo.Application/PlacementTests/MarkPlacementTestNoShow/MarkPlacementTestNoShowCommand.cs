using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.PlacementTests.MarkPlacementTestNoShow;

public sealed class MarkPlacementTestNoShowCommand : ICommand<MarkPlacementTestNoShowResponse>
{
    public Guid PlacementTestId { get; init; }
}

