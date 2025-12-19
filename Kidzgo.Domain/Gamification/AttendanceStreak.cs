using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Gamification;

public class AttendanceStreak : Entity
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public DateOnly AttendanceDate { get; set; }
    public int CurrentStreak { get; set; } // Current consecutive days streak
    public int RewardStars { get; set; } // Stars awarded for this attendance (e.g., 1)
    public int RewardExp { get; set; } // Exp awarded for this attendance (e.g., 5)
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Profile StudentProfile { get; set; } = null!;
}

