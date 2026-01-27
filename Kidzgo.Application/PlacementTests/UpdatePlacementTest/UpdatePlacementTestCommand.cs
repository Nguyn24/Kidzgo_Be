using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.PlacementTests.UpdatePlacementTest;

public sealed class UpdatePlacementTestCommand : ICommand<UpdatePlacementTestResponse>
{
    public Guid PlacementTestId { get; init; }
    public DateTime? ScheduledAt { get; init; }
    public string? Room { get; init; }
    public Guid? InvigilatorUserId { get; init; }
    public Guid? StudentProfileId { get; init; }
    public Guid? ClassId { get; init; }
}

