using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.ToggleRewardStoreItemStatus;

public sealed class ToggleRewardStoreItemStatusCommand : ICommand<ToggleRewardStoreItemStatusResponse>
{
    public Guid Id { get; init; }
}

