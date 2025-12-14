using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;

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

    // Navigation properties
    public Branch Branch { get; set; } = null!;
}
