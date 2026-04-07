using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportRequests.Shared;

namespace Kidzgo.Application.ReportRequests.CompleteReportRequest;

public sealed class CompleteReportRequestCommand : ICommand<ReportRequestDto>
{
    public Guid Id { get; init; }
    public Guid? LinkedSessionReportId { get; init; }
    public Guid? LinkedMonthlyReportId { get; init; }
}
