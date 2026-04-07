using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Reports;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportRequests.Shared;

public static class ReportRequestWorkflow
{
    public static async Task MarkMatchingSessionRequestSubmittedAsync(
        IDbContext context,
        SessionReport sessionReport,
        Guid classId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var request = await context.ReportRequests
            .Where(r => r.ReportType == ReportRequestType.Session &&
                        r.AssignedTeacherUserId == sessionReport.TeacherUserId &&
                        r.TargetStudentProfileId == sessionReport.StudentProfileId &&
                        (r.TargetSessionId == null || r.TargetSessionId == sessionReport.SessionId) &&
                        (r.TargetClassId == null || r.TargetClassId == classId) &&
                        (r.Status == ReportRequestStatus.Requested || r.Status == ReportRequestStatus.InProgress))
            .OrderBy(r => r.Priority == ReportRequestPriority.Urgent ? 0 :
                          r.Priority == ReportRequestPriority.High ? 1 :
                          r.Priority == ReportRequestPriority.Normal ? 2 : 3)
            .ThenBy(r => r.DueAt == null)
            .ThenBy(r => r.DueAt)
            .ThenBy(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (request is null)
        {
            return;
        }

        request.Status = ReportRequestStatus.Submitted;
        request.LinkedSessionReportId = sessionReport.Id;
        request.SubmittedAt = now;
        request.UpdatedAt = now;
    }

    public static async Task MarkMatchingMonthlyRequestSubmittedAsync(
        IDbContext context,
        StudentMonthlyReport report,
        Guid submittedBy,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var request = await context.ReportRequests
            .Where(r => r.ReportType == ReportRequestType.Monthly &&
                        r.AssignedTeacherUserId == submittedBy &&
                        r.TargetStudentProfileId == report.StudentProfileId &&
                        (r.TargetClassId == null || r.TargetClassId == report.ClassId) &&
                        (r.Month == null || r.Month == report.Month) &&
                        (r.Year == null || r.Year == report.Year) &&
                        (r.Status == ReportRequestStatus.Requested || r.Status == ReportRequestStatus.InProgress))
            .OrderBy(r => r.Priority == ReportRequestPriority.Urgent ? 0 :
                          r.Priority == ReportRequestPriority.High ? 1 :
                          r.Priority == ReportRequestPriority.Normal ? 2 : 3)
            .ThenBy(r => r.DueAt == null)
            .ThenBy(r => r.DueAt)
            .ThenBy(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (request is null)
        {
            return;
        }

        request.Status = ReportRequestStatus.Submitted;
        request.LinkedMonthlyReportId = report.Id;
        request.SubmittedAt = now;
        request.UpdatedAt = now;
    }

    public static Task MarkSessionRequestReviewedAsync(
        IDbContext context,
        Guid sessionReportId,
        ReportRequestStatus status,
        CancellationToken cancellationToken)
    {
        return MarkReviewedAsync(
            context.ReportRequests.Where(r => r.LinkedSessionReportId == sessionReportId),
            status,
            cancellationToken);
    }

    public static Task MarkMonthlyRequestReviewedAsync(
        IDbContext context,
        Guid monthlyReportId,
        ReportRequestStatus status,
        CancellationToken cancellationToken)
    {
        return MarkReviewedAsync(
            context.ReportRequests.Where(r => r.LinkedMonthlyReportId == monthlyReportId),
            status,
            cancellationToken);
    }

    private static async Task MarkReviewedAsync(
        IQueryable<ReportRequest> query,
        ReportRequestStatus status,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var requests = await query
            .Where(r => r.Status == ReportRequestStatus.Requested ||
                        r.Status == ReportRequestStatus.InProgress ||
                        r.Status == ReportRequestStatus.Submitted)
            .ToListAsync(cancellationToken);

        foreach (var request in requests)
        {
            request.Status = status;
            request.UpdatedAt = now;
        }
    }
}
