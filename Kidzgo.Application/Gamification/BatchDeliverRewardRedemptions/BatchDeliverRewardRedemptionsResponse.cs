namespace Kidzgo.Application.Gamification.BatchDeliverRewardRedemptions;

public sealed class BatchDeliverRewardRedemptionsResponse
{
    /// <summary>
    /// Số lượng redemption đã được deliver
    /// </summary>
    public int DeliveredCount { get; init; }

    /// <summary>
    /// Danh sách ID các redemption đã được deliver
    /// </summary>
    public List<Guid> DeliveredRedemptionIds { get; init; } = new();

    /// <summary>
    /// Thời gian deliver
    /// </summary>
    public DateTime DeliveredAt { get; init; }
}

