using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Sessions.Errors;

public static class AttendanceErrors
{
    public static Error NotFound(Guid? id) => Error.NotFound(
        "Attendance.NotFound",
        $"The attendance record with Id = '{id}' was not found.");
}

