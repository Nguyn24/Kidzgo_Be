using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.ConfirmReceivedRewardRedemption;

public sealed class ConfirmReceivedRewardRedemptionCommand : ICommand<ConfirmReceivedRewardRedemptionResponse>
{
    public Guid Id { get; init; }
}

