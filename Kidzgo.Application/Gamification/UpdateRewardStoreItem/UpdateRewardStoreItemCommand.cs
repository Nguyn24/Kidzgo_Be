using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.UpdateRewardStoreItem;

public sealed class UpdateRewardStoreItemCommand : ICommand<UpdateRewardStoreItemResponse>
{
    public Guid Id { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public int? CostStars { get; init; }
    public int? Quantity { get; init; }
    public bool? IsActive { get; init; }
}

