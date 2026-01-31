namespace Kidzgo.Application.Gamification.GetRewardStoreItemById;

public sealed class GetRewardStoreItemByIdResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public int CostStars { get; init; }
    public int Quantity { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

