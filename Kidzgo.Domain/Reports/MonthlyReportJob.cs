using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Reports;

public class MonthlyReportJob : Entity
{
    public Guid Id { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public Guid BranchId { get; set; }
    public MonthlyReportJobStatus Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string? AiPayloadRef { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Branch Branch { get; set; } = null!;
    public User? CreatedByUser { get; set; }
    public ICollection<StudentMonthlyReport> Reports { get; set; } = new List<StudentMonthlyReport>();
}
