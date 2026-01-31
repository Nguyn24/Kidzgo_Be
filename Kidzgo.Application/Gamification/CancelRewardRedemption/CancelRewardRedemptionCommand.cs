using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.CancelRewardRedemption;

public sealed class CancelRewardRedemptionCommand : ICommand<CancelRewardRedemptionResponse>
{
    public Guid Id { get; init; }
    public string? Reason { get; init; }
}

