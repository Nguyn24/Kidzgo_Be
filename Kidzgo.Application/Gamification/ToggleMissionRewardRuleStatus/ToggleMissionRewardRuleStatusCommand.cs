using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.ToggleMissionRewardRuleStatus;

public sealed class ToggleMissionRewardRuleStatusCommand : ICommand<ToggleMissionRewardRuleStatusResponse>
{
    public Guid Id { get; init; }
}
