using Kidzgo.Domain.Gamification;

namespace Kidzgo.Application.Missions.CreateMission;

public sealed class CreateMissionResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public string Scope { get; init; } = null!;
    public Guid? TargetClassId { get; init; }
    public string? TargetGroup { get; init; }
    public string MissionType { get; init; } = null!;
    public DateTime? StartAt { get; init; }
    public DateTime? EndAt { get; init; }
    public int? RewardStars { get; init; }
    public int? RewardExp { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTime CreatedAt { get; init; }
}

