namespace Kidzgo.Application.Gamification.RequestRewardRedemption;

public sealed class RequestRewardRedemptionResponse
{
    public Guid Id { get; init; }
    public Guid ItemId { get; init; }
    public string ItemName { get; init; } = null!;
    public Guid StudentProfileId { get; init; }
    public string Status { get; init; } = null!;
    public int StarsDeducted { get; init; }
    public int RemainingStars { get; init; }
    public DateTime CreatedAt { get; init; }
}

