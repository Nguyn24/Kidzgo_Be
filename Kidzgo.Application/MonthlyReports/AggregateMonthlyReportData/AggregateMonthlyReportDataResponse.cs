namespace Kidzgo.Application.MonthlyReports.AggregateMonthlyReportData;

public sealed class AggregateMonthlyReportDataResponse
{
    public int TotalReportsCreated { get; init; }
    public int TotalReportsUpdated { get; init; }
    public List<Guid> ReportIds { get; init; } = new();
}

