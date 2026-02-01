using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.ApproveRewardRedemption;

public sealed class ApproveRewardRedemptionCommand : ICommand<ApproveRewardRedemptionResponse>
{
    public Guid Id { get; init; }
}

