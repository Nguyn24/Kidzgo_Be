namespace Kidzgo.Application.Gamification.MarkDeliveredRewardRedemption;

public sealed class MarkDeliveredRewardRedemptionResponse
{
    public Guid Id { get; init; }
    public string Status { get; init; } = null!;
    public DateTime DeliveredAt { get; init; }
}

