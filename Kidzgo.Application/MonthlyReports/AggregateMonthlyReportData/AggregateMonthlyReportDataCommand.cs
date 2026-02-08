using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.MonthlyReports.AggregateMonthlyReportData;

/// <summary>
/// UC-175: Gom dữ liệu cho Monthly Report
/// </summary>
public sealed class AggregateMonthlyReportDataCommand : ICommand<AggregateMonthlyReportDataResponse>
{
    public Guid JobId { get; init; }
}

