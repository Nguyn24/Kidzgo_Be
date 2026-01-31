namespace Kidzgo.Application.Gamification.GetRewardRedemptionById;

public sealed class GetRewardRedemptionByIdResponse
{
    public Guid Id { get; init; }
    public Guid ItemId { get; init; }
    public string ItemName { get; init; } = null!;
    public Guid StudentProfileId { get; init; }
    public string? StudentName { get; init; }
    public string Status { get; init; } = null!;
    public Guid? HandledBy { get; init; }
    public string? HandledByName { get; init; }
    public DateTime? HandledAt { get; init; }
    public DateTime? DeliveredAt { get; init; }
    public DateTime? ReceivedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

