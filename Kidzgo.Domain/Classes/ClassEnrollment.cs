using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Domain.Classes;

public class ClassEnrollment : Entity
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid StudentProfileId { get; set; }
    public DateOnly EnrollDate { get; set; }
    public EnrollmentStatus Status { get; set; }
    public Guid? TuitionPlanId { get; set; }
    public Guid? RegistrationId { get; set; }
    public RegistrationTrackType Track { get; set; } = RegistrationTrackType.Primary;
    public string? SessionSelectionPattern { get; set; }
    public string? EnrollmentConfirmationPdfUrl { get; set; }
    public DateTime? EnrollmentConfirmationPdfGeneratedAt { get; set; }
    public Guid? EnrollmentConfirmationPdfGeneratedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Class Class { get; set; } = null!;
    public Profile StudentProfile { get; set; } = null!;
    public TuitionPlan? TuitionPlan { get; set; }
    public Registration? Registration { get; set; }
    public ICollection<StudentSessionAssignment> StudentSessionAssignments { get; set; } = new List<StudentSessionAssignment>();
}
