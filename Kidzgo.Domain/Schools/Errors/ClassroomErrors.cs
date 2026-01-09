using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Schools.Errors;

public static class ClassroomErrors
{
    public static Error NotFound(Guid? classroomId) => Error.NotFound(
        "Classroom.NotFound",
        $"Classroom with Id = '{classroomId}' was not found");

    public static readonly Error BranchNotFound = Error.NotFound(
        "Classroom.BranchNotFound",
        "Branch not found or inactive");

    public static readonly Error HasSessions = Error.Conflict(
        "Classroom.HasSessions",
        "Cannot delete classroom that is being used in sessions");
}

