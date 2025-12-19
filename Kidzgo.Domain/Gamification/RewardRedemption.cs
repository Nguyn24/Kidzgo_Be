using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Gamification;

public class RewardRedemption : Entity
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = null!; // Store item name at redemption time
    public Guid StudentProfileId { get; set; }
    public RedemptionStatus Status { get; set; }
    public Guid? HandledBy { get; set; }
    public DateTime? HandledAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public RewardStoreItem Item { get; set; } = null!;
    public Profile StudentProfile { get; set; } = null!;
    public User? HandledByUser { get; set; }
}
