namespace Kidzgo.Application.MonthlyReports.PublishMonthlyReport;

public sealed class PublishMonthlyReportResponse
{
    public Guid Id { get; init; }
    public string Status { get; init; } = null!;
    public DateTime? PublishedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

