using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Gamification;

public class RewardStoreItem : Entity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int CostStars { get; set; }
    public int Quantity { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<RewardRedemption> RewardRedemptions { get; set; } = new List<RewardRedemption>();
}
