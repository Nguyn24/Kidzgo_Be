using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Classes.Errors;

public static class ClassErrors
{
    public static Error NotFound(Guid? classId) => Error.NotFound(
        "Class.NotFound",
        $"Class with Id = '{classId}' was not found");

    public static readonly Error BranchNotFound = Error.NotFound(
        "Class.BranchNotFound",
        "Branch not found or inactive");

    public static readonly Error ProgramNotFound = Error.NotFound(
        "Class.ProgramNotFound",
        "Program not found, deleted, or inactive");

    public static readonly Error CodeExists = Error.Conflict(
        "Class.CodeExists",
        "Class code already exists");

    public static readonly Error MainTeacherNotFound = Error.NotFound(
        "Class.MainTeacherNotFound",
        "Main teacher not found or is not a teacher");

    public static readonly Error MainTeacherBranchMismatch = Error.Conflict(
        "Class.MainTeacherBranchMismatch",
        "Main teacher must belong to the same branch as the class");

    public static readonly Error AssistantTeacherNotFound = Error.NotFound(
        "Class.AssistantTeacherNotFound",
        "Assistant teacher not found or is not a teacher");

    public static readonly Error AssistantTeacherBranchMismatch = Error.Conflict(
        "Class.AssistantTeacherBranchMismatch",
        "Assistant teacher must belong to the same branch as the class");

    public static readonly Error HasActiveEnrollments = Error.Conflict(
        "Class.HasActiveEnrollments",
        "Cannot delete class with active enrollments");

    public static readonly Error StatusUnchanged = Error.Validation(
        "Class.StatusUnchanged",
        "Class status is already set to the requested status");

    public static readonly Error InvalidStatusTransition = Error.Validation(
        "Class.InvalidStatusTransition",
        "Cannot change status from Closed to Planned");

    // Session conflict errors
    public static Error RoomConflict(string classCode, string classTitle, DateTime conflictDatetime) => Error.Conflict(
        "Class.RoomConflict",
        $"Room is already booked by class '{classCode} - {classTitle}' at {conflictDatetime:dd/MM/yyyy HH:mm}");

    public static Error TeacherConflict(string classCode, string classTitle, DateTime conflictDatetime, string? roomName = null) => Error.Conflict(
        "Class.TeacherConflict",
        roomName != null
            ? $"Teacher is already assigned to class '{classCode} - {classTitle}' at {conflictDatetime:dd/MM/yyyy HH:mm} in room '{roomName}'"
            : $"Teacher is already assigned to class '{classCode} - {classTitle}' at {conflictDatetime:dd/MM/yyyy HH:mm}");

    public static Error AssistantConflict(string classCode, string classTitle, DateTime conflictDatetime) => Error.Conflict(
        "Class.AssistantConflict",
        $"Assistant teacher is already assigned to class '{classCode} - {classTitle}' at {conflictDatetime:dd/MM/yyyy HH:mm}");
}

public static class ScheduleErrors
{
    public static Error Empty => Error.Validation(
        "SchedulePattern.Empty",
        "Schedule pattern cannot be empty");

    public static Error Invalid(string message) => Error.Validation(
        "SchedulePattern.Invalid",
        $"Invalid RRULE pattern: {message}");
}
