namespace Kidzgo.Application.Gamification.ConfirmReceivedRewardRedemption;

public sealed class ConfirmReceivedRewardRedemptionResponse
{
    public Guid Id { get; init; }
    public string Status { get; init; } = null!;
    public DateTime ReceivedAt { get; init; }
}

