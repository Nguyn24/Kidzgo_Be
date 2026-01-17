using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Attendance.GetStudentAttendanceHistory;

public sealed class GetStudentAttendanceHistoryResponse
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public DateTime SessionDateTime { get; set; }
    public AttendanceStatus AttendanceStatus { get; set; }
    public AbsenceType? AbsenceType { get; set; }
}

