using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.PlacementTests.GetPlacementTestById;

public sealed class GetPlacementTestByIdQuery : IQuery<GetPlacementTestByIdResponse>
{
    public Guid PlacementTestId { get; init; }
}

