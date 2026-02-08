using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.MonthlyReports.GetMonthlyReportJobById;

/// <summary>
/// UC-178: Xem trạng thái Monthly Report Job
/// </summary>
public sealed class GetMonthlyReportJobByIdQuery : IQuery<GetMonthlyReportJobByIdResponse>
{
    public Guid JobId { get; init; }
}

