namespace Kidzgo.Application.MonthlyReports.SubmitMonthlyReport;

public sealed class SubmitMonthlyReportResponse
{
    public Guid Id { get; init; }
    public string Status { get; init; } = null!;
    public Guid? SubmittedBy { get; init; }
    public DateTime UpdatedAt { get; init; }
}

