namespace Kidzgo.Application.PlacementTests.UpdatePlacementTest;

public sealed class UpdatePlacementTestResponse
{
    public Guid Id { get; init; }
    public Guid? LeadId { get; init; }
    public Guid? StudentProfileId { get; init; }
    public Guid? ClassId { get; init; }
    public DateTime? ScheduledAt { get; init; }
    public int DurationMinutes { get; init; }
    public string Status { get; init; } = null!;
    public Guid? RoomId { get; init; }
    public string? RoomName { get; init; }
    public string? Room { get; init; }
    public Guid? InvigilatorUserId { get; init; }
    public DateTime UpdatedAt { get; init; }
}

