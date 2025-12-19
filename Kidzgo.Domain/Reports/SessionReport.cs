using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Domain.Reports;

public class SessionReport : Entity
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid TeacherUserId { get; set; } // Teacher who created the report
    public DateOnly ReportDate { get; set; } // Date of the session for filtering
    public string Feedback { get; set; } = null!; // Teacher's feedback/notes for the student
    public string? AiGeneratedSummary { get; set; } // AI-generated summary (for monthly compilation)
    public bool IsMonthlyCompiled { get; set; } // Whether this has been included in monthly report
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Session Session { get; set; } = null!;
    public Profile StudentProfile { get; set; } = null!;
    public User TeacherUser { get; set; } = null!;
}

