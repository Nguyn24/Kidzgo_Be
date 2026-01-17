using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Attendance.GetSessionAttendance;

public sealed class GetSessionAttendanceQuery : IQuery<List<GetSessionAttendanceResponse>>
{
    public Guid SessionId { get; init; }
}

