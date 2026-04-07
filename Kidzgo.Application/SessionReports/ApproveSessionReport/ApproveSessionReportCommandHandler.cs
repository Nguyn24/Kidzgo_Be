using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportRequests.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.SessionReports.ApproveSessionReport;

public sealed class ApproveSessionReportCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<ApproveSessionReportCommand, ApproveSessionReportResponse>
{
    public async Task<Result<ApproveSessionReportResponse>> Handle(
        ApproveSessionReportCommand command,
        CancellationToken cancellationToken)
    {
        var sessionReport = await context.SessionReports
            .Include(sr => sr.StudentProfile)
            .Include(sr => sr.TeacherUser)
            .FirstOrDefaultAsync(sr => sr.Id == command.SessionReportId, cancellationToken);

        if (sessionReport is null)
        {
            return Result.Failure<ApproveSessionReportResponse>(
                SessionReportErrors.NotFound(command.SessionReportId));
        }

        // Only REVIEW status can be approved
        if (sessionReport.Status != ReportStatus.Review)
        {
            return Result.Failure<ApproveSessionReportResponse>(
                SessionReportErrors.InvalidStatusForOperation(
                    sessionReport.Status,
                    "approve"));
        }

        // Update status to Approved
        sessionReport.Status = ReportStatus.Approved;
        sessionReport.ReviewedByUserId = userContext.UserId;
        sessionReport.ReviewedAt = DateTime.UtcNow;
        sessionReport.UpdatedAt = DateTime.UtcNow;

        await ReportRequestWorkflow.MarkSessionRequestReviewedAsync(
            context,
            sessionReport.Id,
            ReportRequestStatus.Approved,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        // Get reviewer name
        var reviewer = await context.Users.FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        return new ApproveSessionReportResponse
        {
            Id = sessionReport.Id,
            SessionId = sessionReport.SessionId,
            StudentProfileId = sessionReport.StudentProfileId,
            StudentName = sessionReport.StudentProfile.DisplayName,
            TeacherUserId = sessionReport.TeacherUserId,
            TeacherName = sessionReport.TeacherUser.Name,
            Status = sessionReport.Status,
            ReportDate = sessionReport.ReportDate,
            Feedback = sessionReport.Feedback,
            ReviewedBy = sessionReport.ReviewedByUserId,
            ReviewedByName = reviewer?.Name,
            ReviewedAt = sessionReport.ReviewedAt,
            UpdatedAt = sessionReport.UpdatedAt
        };
    }
}
