using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Classes;

namespace Kidzgo.Domain.Reports;

public class StudentMonthlyReport : Entity
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? JobId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public string? DraftContent { get; set; }
    public string? FinalContent { get; set; }
    public ReportStatus Status { get; set; }
    public string? AiVersion { get; set; }
    public string? PdfUrl { get; set; }
    public DateTime? PdfGeneratedAt { get; set; }
    public Guid? SubmittedBy { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Profile StudentProfile { get; set; } = null!;
    public Class? Class { get; set; }
    public MonthlyReportJob? Job { get; set; }
    public User? SubmittedByUser { get; set; }
    public User? ReviewedByUser { get; set; }
    public ICollection<ReportComment> ReportComments { get; set; } = new List<ReportComment>();
}
