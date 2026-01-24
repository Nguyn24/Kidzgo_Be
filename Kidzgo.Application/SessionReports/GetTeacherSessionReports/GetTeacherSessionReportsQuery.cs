using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.SessionReports.GetTeacherSessionReports;

public sealed class GetTeacherSessionReportsQuery : IQuery<GetTeacherSessionReportsResponse>, IPageableQuery
{
    public Guid TeacherUserId { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

