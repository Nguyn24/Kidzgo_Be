namespace Kidzgo.Application.Gamification.ApproveRewardRedemption;

public sealed class ApproveRewardRedemptionResponse
{
    public Guid Id { get; init; }
    public string Status { get; init; } = null!;
    public Guid HandledBy { get; init; }
    public DateTime HandledAt { get; init; }
}

