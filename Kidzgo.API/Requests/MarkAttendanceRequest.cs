using Kidzgo.Domain.Sessions;

namespace Kidzgo.API.Requests;

public sealed class MarkAttendanceRequest
{
    public List<Application.Attendance.MarkAttendance.StudentAttendanceItem> Attendances { get; init; } = new();
}

public sealed class StudentAttendanceItem
{
    public Guid StudentProfileId { get; set; }
    public AttendanceStatus AttendanceStatus { get; set; }
    public string? Note { get; set; }
}
