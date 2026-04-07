using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.ReportRequests.Shared;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.ReportRequests.GetReportRequests;

public sealed class GetReportRequestsResponse
{
    public Page<ReportRequestDto> ReportRequests { get; init; } = null!;
}
