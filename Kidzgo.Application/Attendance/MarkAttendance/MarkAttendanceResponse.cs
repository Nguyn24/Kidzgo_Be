using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Attendance.MarkAttendance;

public sealed class MarkAttendanceResponse
{
    public Guid Id { get; init; }
    public Guid SessionId { get; init; }
    public Guid StudentProfileId { get; init; }
    public AttendanceStatus AttendanceStatus { get; init; }
    public AbsenceType? AbsenceType { get; init; }
    public DateTime? MarkedAt { get; init; }
}

