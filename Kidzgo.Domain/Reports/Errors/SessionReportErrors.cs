using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Reports.Errors;

public static class SessionReportErrors
{
    public static Error NotFound(Guid? sessionReportId) => Error.NotFound(
        "SessionReport.NotFound",
        $"Session report with Id = '{sessionReportId}' was not found");

    public static readonly Error AlreadyExists = Error.Conflict(
        "SessionReport.AlreadyExists",
        "A session report already exists for this session and student. Use update instead.");

    public static Error SessionNotEnded(DateTime sessionEndAt) => Error.Validation(
        "SessionReport.SessionNotEnded",
        $"Session report can only be created, edited, submitted, approved, or published after the session has ended at '{sessionEndAt:yyyy-MM-dd HH:mm:ss}' UTC.");

    public static Error InvalidStatusForOperation(ReportStatus currentStatus, string operation) => Error.Validation(
        "SessionReport.InvalidStatus",
        $"Cannot {operation} session report in '{currentStatus}' status.");
}

