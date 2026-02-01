namespace Kidzgo.Application.Gamification.CancelRewardRedemption;

public sealed class CancelRewardRedemptionResponse
{
    public Guid Id { get; init; }
    public string Status { get; init; } = null!;
    public Guid HandledBy { get; init; }
    public DateTime HandledAt { get; init; }
}

