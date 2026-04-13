namespace Kidzgo.Application.Gamification.GetMissionRewardRuleById;

public sealed class GetMissionRewardRuleByIdResponse
{
    public Guid Id { get; init; }
    public string MissionType { get; init; } = null!;
    public string ProgressMode { get; init; } = null!;
    public int TotalRequired { get; init; }
    public int RewardStars { get; init; }
    public int RewardExp { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
