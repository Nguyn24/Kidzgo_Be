using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Attendance.GetSessionAttendance;

public sealed class GetSessionAttendanceResponse
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public string StudentName { get; set; } = null!;
    public AttendanceStatus AttendanceStatus { get; set; }
    public AbsenceType? AbsenceType { get; set; }
    public bool HasMakeupCredit { get; set; }
}

