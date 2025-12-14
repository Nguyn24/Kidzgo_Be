using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Gamification;

public class StudentLevel : Entity
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public string CurrentLevel { get; set; } = null!;
    public int CurrentXp { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Profile StudentProfile { get; set; } = null!;
}
