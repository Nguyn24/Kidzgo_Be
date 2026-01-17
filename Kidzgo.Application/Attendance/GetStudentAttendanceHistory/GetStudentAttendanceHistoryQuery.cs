using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Attendance.GetStudentAttendanceHistory;

public sealed class GetStudentAttendanceHistoryQuery : IPageableQuery, IQuery<Page<GetStudentAttendanceHistoryResponse>>
{
    public Guid StudentProfileId { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

