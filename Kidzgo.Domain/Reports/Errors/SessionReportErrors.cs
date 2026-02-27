using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;

namespace Kidzgo.Domain.Reports.Errors;

public static class SessionReportErrors
{
    public static Error NotFound(Guid? sessionReportId) => Error.NotFound(
        "SessionReport.NotFound",
        $"Session report with Id = '{sessionReportId}' was not found");

    public static readonly Error AlreadyExists = Error.Conflict(
        "SessionReport.AlreadyExists",
        "A session report already exists for this session and student. Use update instead.");

    public static Error InvalidStatusForOperation(ReportStatus currentStatus, string operation) => Error.Validation(
        "SessionReport.InvalidStatus",
        $"Cannot {operation} session report in '{currentStatus}' status.");
}
