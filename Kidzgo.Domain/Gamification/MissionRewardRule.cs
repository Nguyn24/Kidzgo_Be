using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Gamification;

public class MissionRewardRule : Entity
{
    public Guid Id { get; set; }
    public MissionType MissionType { get; set; }
    public MissionProgressMode ProgressMode { get; set; } = MissionProgressMode.Count;
    public int TotalRequired { get; set; }
    public int RewardStars { get; set; }
    public int RewardExp { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
