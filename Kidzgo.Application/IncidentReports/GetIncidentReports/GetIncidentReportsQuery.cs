using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Tickets;

namespace Kidzgo.Application.IncidentReports.GetIncidentReports;

public sealed class GetIncidentReportsQuery : IQuery<GetIncidentReportsResponse>, IPageableQuery
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
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
