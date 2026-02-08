using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.MonthlyReports.GetMonthlyReportById;

/// <summary>
/// UC-179: Teacher xem draft Monthly Report
/// UC-186: Parent/Student xem Monthly Report
/// </summary>
public sealed class GetMonthlyReportByIdQuery : IQuery<GetMonthlyReportByIdResponse>
{
    public Guid ReportId { get; init; }
}

