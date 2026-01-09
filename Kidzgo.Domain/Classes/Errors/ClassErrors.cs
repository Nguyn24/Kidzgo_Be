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
}

