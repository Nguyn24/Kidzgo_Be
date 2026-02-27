using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Reports;

public class ReportComment : Entity
{
    public Guid Id { get; set; }
    public Guid? ReportId { get; set; } // For StudentMonthlyReport comments (nullable)
    public Guid? SessionReportId { get; set; } // For SessionReport comments (nullable)
    public Guid CommenterId { get; set; }
    public string Content { get; set; }= null!;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public StudentMonthlyReport? Report { get; set; }
    public SessionReport? SessionReport { get; set; }
    public User CommenterUser { get; set; } = null!;
}