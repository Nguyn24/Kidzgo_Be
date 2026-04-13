using Kidzgo.Domain.Gamification;

namespace Kidzgo.API.Requests;

public sealed class CreateMissionRewardRuleRequest
{
    public MissionType MissionType { get; set; }
    public MissionProgressMode ProgressMode { get; set; } = MissionProgressMode.Count;
    public int TotalRequired { get; set; }
    public int RewardStars { get; set; }
    public int RewardExp { get; set; }
    public bool IsActive { get; set; } = true;
}
