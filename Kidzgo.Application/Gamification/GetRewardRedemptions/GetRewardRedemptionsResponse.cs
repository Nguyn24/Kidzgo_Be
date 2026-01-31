using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Gamification.GetRewardRedemptions;

public sealed class GetRewardRedemptionsResponse
{
    public Page<RewardRedemptionDto> Redemptions { get; init; } = null!;
}

public sealed class RewardRedemptionDto
{
    public Guid Id { get; init; }
    public Guid ItemId { get; init; }
    public string ItemName { get; init; } = null!;
    public Guid StudentProfileId { get; init; }
    public string? StudentName { get; init; }
    public string Status { get; init; } = null!;
    public Guid? HandledBy { get; init; }
    public string? HandledByName { get; init; }
    public DateTime? HandledAt { get; init; }
    public DateTime? DeliveredAt { get; init; }
    public DateTime? ReceivedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

