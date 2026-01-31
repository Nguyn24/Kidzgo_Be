using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.GetRewardStoreItems;

public sealed class GetRewardStoreItemsQuery : IQuery<GetRewardStoreItemsResponse>
{
    public bool? IsActive { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

