using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Gamification;

public class StarTransaction : Entity
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public int Amount { get; set; }
    public string? Reason { get; set; }
    public StarSourceType SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public int BalanceAfter { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Profile StudentProfile { get; set; } = null!;
    public User? CreatedByUser { get; set; }
}
