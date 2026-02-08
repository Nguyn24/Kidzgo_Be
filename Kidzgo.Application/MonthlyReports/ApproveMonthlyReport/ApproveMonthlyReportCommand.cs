using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.MonthlyReports.ApproveMonthlyReport;

/// <summary>
/// UC-183: Staff/Admin approve Monthly Report
/// </summary>
public sealed class ApproveMonthlyReportCommand : ICommand<ApproveMonthlyReportResponse>
{
    public Guid ReportId { get; init; }
}

