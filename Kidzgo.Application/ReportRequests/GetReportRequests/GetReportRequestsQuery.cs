using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.ReportRequests.GetReportRequests;

public sealed class GetReportRequestsQuery : IQuery<GetReportRequestsResponse>, IPageableQuery
{
    public ReportRequestType? ReportType { get; init; }
    public ReportRequestStatus? Status { get; init; }
    public ReportRequestPriority? Priority { get; init; }
    public Guid? AssignedTeacherUserId { get; init; }
    public Guid? TargetStudentProfileId { get; init; }
    public Guid? TargetClassId { get; init; }
    public int? Month { get; init; }
    public int? Year { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
