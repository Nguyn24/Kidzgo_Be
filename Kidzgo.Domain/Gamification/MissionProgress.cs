using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Gamification;

public class MissionProgress : Entity
{
    public Guid Id { get; set; }
    public Guid MissionId { get; set; }
    public Guid StudentProfileId { get; set; }
    public MissionProgressStatus Status { get; set; }
    public decimal? ProgressValue { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? VerifiedBy { get; set; }

    // Navigation properties
    public Mission Mission { get; set; } = null!;
    public Profile StudentProfile { get; set; } = null!;
    public User? VerifiedByUser { get; set; }
}
