using Kidzgo.Application.IncidentReports.Shared;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.IncidentReports.GetIncidentReports;

public sealed class GetIncidentReportsResponse
{
    public Page<IncidentReportDto> IncidentReports { get; init; } = null!;
}
