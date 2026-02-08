using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.MonthlyReports.RejectMonthlyReport;

/// <summary>
/// UC-184: Staff/Admin reject Monthly Report
/// </summary>
public sealed class RejectMonthlyReportCommand : ICommand<RejectMonthlyReportResponse>
{
    public Guid ReportId { get; init; }
}

