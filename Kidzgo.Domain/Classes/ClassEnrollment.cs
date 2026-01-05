using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Programs;

namespace Kidzgo.Domain.Classes;

public class ClassEnrollment : Entity
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid StudentProfileId { get; set; }
    public DateOnly EnrollDate { get; set; }
    public EnrollmentStatus Status { get; set; }
    public Guid? TuitionPlanId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Class Class { get; set; } = null!;
    public Profile StudentProfile { get; set; } = null!;
    public TuitionPlan? TuitionPlan { get; set; }
}
