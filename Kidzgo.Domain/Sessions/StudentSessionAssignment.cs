using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Sessions;

public class StudentSessionAssignment : Entity
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid ClassEnrollmentId { get; set; }
    public Guid? RegistrationId { get; set; }
    public RegistrationTrackType Track { get; set; }
    public StudentSessionAssignmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Session Session { get; set; } = null!;
    public Profile StudentProfile { get; set; } = null!;
    public ClassEnrollment ClassEnrollment { get; set; } = null!;
    public Registration? Registration { get; set; }
}
