using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Gamification.Errors;

public static class MissionErrors
{
    public static Error NotFound(Guid? missionId) => Error.NotFound(
        "Mission.NotFound",
        $"Mission with Id = '{missionId}' was not found");

    public static readonly Error ClassNotFound = Error.NotFound(
        "Mission.ClassNotFound",
        "Target class not found");

    public static readonly Error StudentNotFound = Error.NotFound(
        "Mission.StudentNotFound",
        "Target student not found");

    public static Error SomeStudentsNotFound(int count) => Error.NotFound(
        "Mission.SomeStudentsNotFound",
        $"{count} student(s) not found");

    public static readonly Error InvalidScope = Error.Validation(
        "Mission.InvalidScope",
        "Invalid mission scope. For Class scope, TargetClassId is required. For Student scope, TargetStudentId is required. For Group scope, TargetGroup is required.");

    public static readonly Error InvalidDateRange = Error.Validation(
        "Mission.InvalidDateRange",
        "EndAt must be after StartAt");

    public static readonly Error TeacherCannotTargetClass = Error.Validation(
        "Mission.TeacherCannotTargetClass",
        "Teacher can only target classes they are assigned to.");

    public static readonly Error TeacherCannotTargetStudent = Error.Validation(
        "Mission.TeacherCannotTargetStudent",
        "Teacher can only target students in classes they teach.");

    public static Error TeacherCannotTargetSomeStudents(int count) => Error.Validation(
        "Mission.TeacherCannotTargetSomeStudents",
        $"Teacher cannot target {count} student(s) outside classes they teach.");

    public static readonly Error MissionInUse = Error.Conflict(
        "Mission.MissionInUse",
        "Cannot delete mission that has progress records");
}

