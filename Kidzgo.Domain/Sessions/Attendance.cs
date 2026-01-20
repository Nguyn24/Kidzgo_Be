using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Sessions;

public class Attendance : Entity
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid StudentProfileId { get; set; }
    public AttendanceStatus AttendanceStatus { get; set; }
    public AbsenceType? AbsenceType { get; set; }
    public Guid? MarkedBy { get; set; }
    public DateTime? MarkedAt { get; set; }
    public string? Note { get; set; }

    // Navigation properties
    public Session Session { get; set; } = null!;
    public Profile StudentProfile { get; set; } = null!;
    public User? MarkedByUser { get; set; }
}
