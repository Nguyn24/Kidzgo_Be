using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Tickets;

namespace Kidzgo.Application.IncidentReports.GetIncidentReportStatistics;

public sealed class GetIncidentReportStatisticsQuery : IQuery<GetIncidentReportStatisticsResponse>
{
    public Guid? BranchId { get; init; }
    public Guid? OpenedByUserId { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public Guid? ClassId { get; init; }
    public IncidentReportCategory? Category { get; init; }
    public IncidentReportStatus? Status { get; init; }
    public string? Keyword { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
}
