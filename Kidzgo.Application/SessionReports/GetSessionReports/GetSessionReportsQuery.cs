using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.SessionReports.GetSessionReports;

public sealed class GetSessionReportsQuery : IQuery<GetSessionReportsResponse>, IPageableQuery
{
    public Guid? SessionId { get; init; }
    public Guid? StudentProfileId { get; init; }
    public Guid? TeacherUserId { get; init; }
    public Guid? ClassId { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

