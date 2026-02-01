using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.CreateRewardStoreItem;

public sealed class CreateRewardStoreItemCommand : ICommand<CreateRewardStoreItemResponse>
{
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public int CostStars { get; init; }
    public int Quantity { get; init; }
    public bool IsActive { get; init; } = true;
}

