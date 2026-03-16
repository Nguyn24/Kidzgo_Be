using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Attendance.GetSessionAttendance;

public sealed class GetSessionAttendanceQuery : IQuery<GetSessionAttendanceListResponse>
{
    public Guid SessionId { get; init; }
}

