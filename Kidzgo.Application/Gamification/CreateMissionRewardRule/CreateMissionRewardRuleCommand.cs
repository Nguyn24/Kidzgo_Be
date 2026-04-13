using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Gamification;

namespace Kidzgo.Application.Gamification.CreateMissionRewardRule;

public sealed class CreateMissionRewardRuleCommand : ICommand<CreateMissionRewardRuleResponse>
{
    public MissionType MissionType { get; init; }
    public MissionProgressMode ProgressMode { get; init; } = MissionProgressMode.Count;
    public int TotalRequired { get; init; }
    public int RewardStars { get; init; }
    public int RewardExp { get; init; }
    public bool IsActive { get; init; } = true;
}
