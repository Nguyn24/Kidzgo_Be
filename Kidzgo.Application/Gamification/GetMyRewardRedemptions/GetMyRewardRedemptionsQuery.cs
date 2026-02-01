using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.GetMyRewardRedemptions;

public sealed class GetMyRewardRedemptionsQuery : IQuery<GetMyRewardRedemptionsResponse>
{
    public string? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

