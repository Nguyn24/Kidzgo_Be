namespace Kidzgo.Application.PlacementTests.RetakePlacementTest;

public sealed class RetakePlacementTestResponse
{
    public Guid NewPlacementTestId { get; init; }
    public Guid OriginalPlacementTestId { get; init; }
    public Guid StudentProfileId { get; init; }
    public string? OriginalProgramName { get; init; }
    public string? NewProgramName { get; init; }
    public string? OriginalTuitionPlanName { get; init; }
    public string? NewTuitionPlanName { get; init; }
    public int OriginalRemainingSessions { get; init; }
    public string PlacementTestStatus { get; init; } = null!;
    public DateTime? ScheduledAt { get; init; }
    public int DurationMinutes { get; init; }
    public Guid? RoomId { get; init; }
    public string? RoomName { get; init; }
    public string? Room { get; init; }
    public Guid? InvigilatorUserId { get; init; }
    public DateTime CreatedAt { get; init; }
}
