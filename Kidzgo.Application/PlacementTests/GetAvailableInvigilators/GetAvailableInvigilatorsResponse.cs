using Kidzgo.Application.PlacementTests;

namespace Kidzgo.Application.PlacementTests.GetAvailableInvigilators;

public sealed class GetAvailableInvigilatorsResponse
{
    public DateTime ScheduledAt { get; init; }
    public DateTime EndAt { get; init; }
    public int DurationMinutes { get; init; }
    public List<AvailableInvigilatorDto> Items { get; init; } = new();
    public List<AvailableRoomDto> Rooms { get; init; } = new();
}

public sealed class AvailableInvigilatorDto
{
    public Guid UserId { get; init; }
    public string? Name { get; init; }
    public string Email { get; init; } = null!;
    public string Role { get; init; } = null!;
    public Guid? BranchId { get; init; }
    public bool IsAvailable { get; init; }
    public List<PlacementTestScheduleConflict> Conflicts { get; init; } = new();
}

public sealed class AvailableRoomDto
{
    public Guid RoomId { get; init; }
    public string Name { get; init; } = null!;
    public Guid BranchId { get; init; }
    public int Capacity { get; init; }
    public bool IsAvailable { get; init; }
    public List<PlacementTestScheduleConflict> Conflicts { get; init; } = new();
}
