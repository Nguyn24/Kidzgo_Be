using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Gamification;

namespace Kidzgo.Application.Gamification.UpdateMissionRewardRule;

public sealed class UpdateMissionRewardRuleCommand : ICommand<UpdateMissionRewardRuleResponse>
{
    public Guid Id { get; init; }
    public MissionType? MissionType { get; init; }
    public MissionProgressMode? ProgressMode { get; init; }
    public int? TotalRequired { get; init; }
    public int? RewardStars { get; init; }
    public int? RewardExp { get; init; }
    public bool? IsActive { get; init; }
}
