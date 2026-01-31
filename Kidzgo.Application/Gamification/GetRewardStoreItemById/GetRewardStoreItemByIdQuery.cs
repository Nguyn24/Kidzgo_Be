using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.GetRewardStoreItemById;

public sealed class GetRewardStoreItemByIdQuery : IQuery<GetRewardStoreItemByIdResponse>
{
    public Guid Id { get; init; }
}

