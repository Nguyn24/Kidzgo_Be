using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.DeleteRewardStoreItem;

public sealed class DeleteRewardStoreItemCommand : ICommand<DeleteRewardStoreItemResponse>
{
    public Guid Id { get; init; }
}

