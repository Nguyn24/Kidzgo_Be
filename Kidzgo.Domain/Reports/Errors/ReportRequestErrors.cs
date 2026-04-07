using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Reports.Errors;

public static class ReportRequestErrors
{
    public static Error NotFound(Guid? requestId) => Error.NotFound(
        "ReportRequest.NotFound",
        $"Report request with Id = '{requestId}' was not found");

    public static readonly Error TargetRequired = Error.Validation(
        "ReportRequest.TargetRequired",
        "A report request must target at least one student or one class");

    public static readonly Error SessionRequired = Error.Validation(
        "ReportRequest.SessionRequired",
        "A session report request must include sessionId");

    public static readonly Error MonthYearRequired = Error.Validation(
        "ReportRequest.MonthYearRequired",
        "A monthly report request must include month and year");

    public static Error InvalidStatus(ReportRequestStatus currentStatus, string action) => Error.Validation(
        "ReportRequest.InvalidStatus",
        $"Cannot {action} report request with status '{currentStatus}'");

    public static readonly Error TeacherNotFound = Error.NotFound(
        "ReportRequest.TeacherNotFound",
        "Assigned teacher was not found");

    public static readonly Error StudentNotFound = Error.NotFound(
        "ReportRequest.StudentNotFound",
        "Target student was not found");

    public static readonly Error ClassNotFound = Error.NotFound(
        "ReportRequest.ClassNotFound",
        "Target class was not found");

    public static readonly Error SessionNotFound = Error.NotFound(
        "ReportRequest.SessionNotFound",
        "Target session was not found");

    public static readonly Error TeacherNotAssigned = Error.Validation(
        "ReportRequest.TeacherNotAssigned",
        "Assigned teacher is not assigned to the target class or session");

    public static readonly Error StudentNotInClass = Error.Validation(
        "ReportRequest.StudentNotInClass",
        "Target student is not enrolled in the target class");

    public static readonly Error StudentNotInSession = Error.Validation(
        "ReportRequest.StudentNotInSession",
        "Target student is not assigned to the target session");

    public static readonly Error ClassSessionMismatch = Error.Validation(
        "ReportRequest.ClassSessionMismatch",
        "Target class does not match the selected session");

    public static readonly Error MonthlyClassRequiredForStudent = Error.Validation(
        "ReportRequest.MonthlyClassRequiredForStudent",
        "Monthly report request for this student requires classId because the student has none or multiple active classes in the selected month");
}
