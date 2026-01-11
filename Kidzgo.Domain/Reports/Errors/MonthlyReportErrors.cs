using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;

namespace Kidzgo.Domain.Reports.Errors;

public static class MonthlyReportErrors
{
    public static Error NotFound(Guid? reportId) => Error.NotFound(
        "MonthlyReport.NotFound",
        $"Monthly Report with Id = '{reportId}' was not found");

    public static Error JobNotFound(Guid? jobId) => Error.NotFound(
        "MonthlyReportJob.NotFound",
        $"Monthly Report Job with Id = '{jobId}' was not found");

    public static Error AlreadyExists(Guid studentProfileId, int month, int year) => Error.Conflict(
        "MonthlyReport.AlreadyExists",
        $"Monthly Report for student '{studentProfileId}' in {month}/{year} already exists");

    public static Error InvalidStatus(ReportStatus currentStatus, string action) => Error.Validation(
        "MonthlyReport.InvalidStatus",
        $"Cannot {action} Monthly Report with status '{currentStatus}'");

    public static Error AiGenerationFailed(string? errorMessage) => Error.Failure(
        "MonthlyReport.AiGenerationFailed",
        $"AI generation failed: {errorMessage ?? "Unknown error"}");

    public static Error PdfGenerationFailed(string? errorMessage) => Error.Failure(
        "MonthlyReport.PdfGenerationFailed",
        $"PDF generation failed: {errorMessage ?? "Unknown error"}");

    public static Error DataAggregationFailed(string? errorMessage) => Error.Failure(
        "MonthlyReport.DataAggregationFailed",
        $"Data aggregation failed: {errorMessage ?? "Unknown error"}");

    public static Error StudentProfileNotFound(Guid? studentProfileId) => Error.NotFound(
        "MonthlyReport.StudentProfileNotFound",
        $"Student Profile with Id = '{studentProfileId}' was not found");

    public static Error ClassNotFound(Guid? classId) => Error.NotFound(
        "MonthlyReport.ClassNotFound",
        $"Class with Id = '{classId}' was not found");
}

