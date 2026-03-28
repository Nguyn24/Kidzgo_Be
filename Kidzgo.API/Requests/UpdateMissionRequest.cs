using Kidzgo.Domain.Gamification;

namespace Kidzgo.API.Requests;

public sealed class UpdateMissionRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public MissionScope Scope { get; set; }
    public Guid? TargetClassId { get; set; }
    public Guid? TargetStudentId { get; set; }
    public List<Guid>? TargetGroup { get; set; }
    public MissionType MissionType { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public int? RewardStars { get; set; }
    public int? RewardExp { get; set; }
    public int? TotalRequired { get; set; }
}
