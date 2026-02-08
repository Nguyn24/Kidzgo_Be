using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.MonthlyReports.SubmitMonthlyReport;

/// <summary>
/// UC-181: Teacher submit Monthly Report
/// </summary>
public sealed class SubmitMonthlyReportCommand : ICommand<SubmitMonthlyReportResponse>
{
    public Guid ReportId { get; init; }
}

