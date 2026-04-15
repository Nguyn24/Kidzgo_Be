using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.PlacementTests.GetAvailableInvigilators;

public sealed class GetAvailableInvigilatorsQuery : IQuery<GetAvailableInvigilatorsResponse>
{
    public DateTime ScheduledAt { get; init; }
    public int? DurationMinutes { get; init; }
    public Guid? BranchId { get; init; }
    public Guid? ExcludePlacementTestId { get; init; }
    public bool IncludeUnavailable { get; init; }
}
