using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Users;

public class ParentStudentLink : Entity
{
    public Guid Id { get; set; }
    public Guid ParentProfileId { get; set; }
    public Guid StudentProfileId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Profile ParentProfile { get; set; } = null!;
    public Profile StudentProfile { get; set; } = null!;
}
