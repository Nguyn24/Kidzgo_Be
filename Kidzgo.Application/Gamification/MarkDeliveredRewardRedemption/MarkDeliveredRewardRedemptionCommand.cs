using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.MarkDeliveredRewardRedemption;

public sealed class MarkDeliveredRewardRedemptionCommand : ICommand<MarkDeliveredRewardRedemptionResponse>
{
    public Guid Id { get; init; }
}

