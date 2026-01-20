using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Sessions.Errors;

public static class AttendanceErrors
{
    public static Error NotFound(Guid? id) => Error.NotFound(
        "Attendance.NotFound",
        $"The attendance record with Id = '{id}' was not found.");

    public static Error NotFoundForSessionStudent(Guid sessionId, Guid studentProfileId) => Error.NotFound(
        "Attendance.NotFound",
        $"Attendance not found for session '{sessionId}' and student '{studentProfileId}'.");

    public static Error UpdateWindowClosed(Guid sessionId) => Error.Validation(
        "Attendance.UpdateWindowClosed",
        $"Attendance for session '{sessionId}' can only be updated within 24 hours after it ends.");
}

