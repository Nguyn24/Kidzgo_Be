using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.RequestRewardRedemption;

public sealed class RequestRewardRedemptionCommand : ICommand<RequestRewardRedemptionResponse>
{
    public Guid ItemId { get; init; }
    public int Quantity { get; init; } = 1;
}

