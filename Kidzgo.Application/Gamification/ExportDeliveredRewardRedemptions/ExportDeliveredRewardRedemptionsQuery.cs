using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.ExportDeliveredRewardRedemptions;

public sealed class ExportDeliveredRewardRedemptionsQuery : IQuery<ExportDeliveredRewardRedemptionsResponse>
{
    public int? Year { get; init; }
    public int? Month { get; init; }
    public Guid? BranchId { get; init; }
    public Guid? ItemId { get; init; }
}
