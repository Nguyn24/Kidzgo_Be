using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.MonthlyReports.PublishMonthlyReport;

/// <summary>
/// UC-185: Publish Monthly Report
/// </summary>
public sealed class PublishMonthlyReportCommand : ICommand<PublishMonthlyReportResponse>
{
    public Guid ReportId { get; init; }
}

