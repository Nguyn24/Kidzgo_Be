using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Attendance.MarkAttendance;

public sealed class MarkAttendanceResponse
{
    public Guid Id { get; init; }
    public Guid SessionId { get; init; }
    public Guid StudentProfileId { get; init; }
    public string AttendanceStatus { get; init; } = null!;
    public string? AbsenceType { get; init; }
    public DateTime? MarkedAt { get; init; }
}

