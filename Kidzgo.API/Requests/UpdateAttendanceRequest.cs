using Kidzgo.Domain.Sessions;

namespace Kidzgo.API.Requests;

public sealed class UpdateAttendanceRequest
{
    public AttendanceStatus AttendanceStatus { get; set; }
}

