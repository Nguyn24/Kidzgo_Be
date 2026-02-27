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

    // ✅ NEW - Workflow status (Draft → Review → Approved/Rejected → Published)
    public ReportStatus Status { get; set; } = ReportStatus.Draft;

    // ✅ NEW - Content fields (similar to StudentMonthlyReport)
    public string? DraftContent { get; set; }
    public string? FinalContent { get; set; }

    // ✅ NEW - Review workflow
    public Guid? SubmittedByUserId { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    // ✅ NEW - AI tracking
    public string? AiVersion { get; set; }

    // ✅ NEW - Monthly compilation tracking (keep existing)
    public bool IsMonthlyCompiled { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Session Session { get; set; } = null!;
    public Profile StudentProfile { get; set; } = null!;
    public User TeacherUser { get; set; } = null!;
    public User? SubmittedByUser { get; set; }
    public User? ReviewedByUser { get; set; }
    public ICollection<ReportComment> ReportComments { get; set; }= new List<ReportComment>();
}

