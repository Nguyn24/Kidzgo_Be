using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.BatchDeliverRewardRedemptions;

/// <summary>
/// Command để deliver hàng loạt các reward redemptions đã approved
/// </summary>
public sealed class BatchDeliverRewardRedemptionsCommand : ICommand<BatchDeliverRewardRedemptionsResponse>
{
    /// <summary>
    /// Năm để filter (nếu null thì deliver tất cả approved)
    /// </summary>
    public int? Year { get; init; }

    /// <summary>
    /// Tháng để filter (nếu null thì deliver tất cả approved)
    /// </summary>
    public int? Month { get; init; }
}

