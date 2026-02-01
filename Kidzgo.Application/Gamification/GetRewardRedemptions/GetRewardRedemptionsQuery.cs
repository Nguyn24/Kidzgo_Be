using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Gamification;

namespace Kidzgo.Application.Gamification.GetRewardRedemptions;

public sealed class GetRewardRedemptionsQuery : IQuery<GetRewardRedemptionsResponse>
{
    public Guid? StudentProfileId { get; init; }
    public Guid? ItemId { get; init; }
    public RedemptionStatus? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

