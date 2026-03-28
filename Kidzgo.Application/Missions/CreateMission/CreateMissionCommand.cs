using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Gamification;

namespace Kidzgo.Application.Missions.CreateMission;

public sealed class CreateMissionCommand : ICommand<CreateMissionResponse>
{
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public MissionScope Scope { get; init; }
    public Guid? TargetClassId { get; init; }
    public Guid? TargetStudentId { get; init; }
    public List<Guid>? TargetGroup { get; init; }
    public MissionType MissionType { get; init; }
    public DateTime? StartAt { get; init; }
    public DateTime? EndAt { get; init; }
    public int? RewardStars { get; init; }
    public int? RewardExp { get; init; }
    public int? TotalRequired { get; init; }
}
