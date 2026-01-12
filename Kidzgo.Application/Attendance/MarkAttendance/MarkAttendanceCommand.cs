using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Attendance.MarkAttendance;

public sealed class MarkAttendanceCommand : ICommand<MarkAttendanceResponse>
{
    public Guid SessionId { get; init; }
    public Guid StudentProfileId { get; init; }
    public AttendanceStatus AttendanceStatus { get; init; }
}

