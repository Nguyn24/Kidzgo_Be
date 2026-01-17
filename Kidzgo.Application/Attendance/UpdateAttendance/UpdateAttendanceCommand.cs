using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Attendance.UpdateAttendance;

public sealed class UpdateAttendanceCommand : ICommand<UpdateAttendanceResponse>
{
    public Guid Id { get; init; }
    public AttendanceStatus AttendanceStatus { get; init; }
}

