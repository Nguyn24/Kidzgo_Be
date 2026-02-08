namespace Kidzgo.Application.MonthlyReports.ApproveMonthlyReport;

public sealed class ApproveMonthlyReportResponse
{
    public Guid Id { get; init; }
    public string Status { get; init; } = null!;
    public Guid? ReviewedBy { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

