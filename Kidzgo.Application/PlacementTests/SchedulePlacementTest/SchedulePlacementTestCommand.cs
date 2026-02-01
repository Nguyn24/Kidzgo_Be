using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.PlacementTests.SchedulePlacementTest;

public sealed class SchedulePlacementTestCommand : ICommand<SchedulePlacementTestResponse>
{
    public Guid LeadId { get; init; }
    public Guid? StudentProfileId { get; init; }
    public Guid? ClassId { get; init; }
    public DateTime ScheduledAt { get; init; }
    public string? Room { get; init; }
    public Guid? InvigilatorUserId { get; init; }
}

