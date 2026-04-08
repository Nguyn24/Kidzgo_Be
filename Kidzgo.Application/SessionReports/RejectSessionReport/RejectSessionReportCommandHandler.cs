using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportRequests.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.SessionReports.RejectSessionReport;

public sealed class RejectSessionReportCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<RejectSessionReportCommand, RejectSessionReportResponse>
{
    public async Task<Result<RejectSessionReportResponse>> Handle(
        RejectSessionReportCommand command,
        CancellationToken cancellationToken)
    {
        var sessionReport = await context.SessionReports
            .Include(sr => sr.StudentProfile)
            .Include(sr => sr.TeacherUser)
            .Include(sr => sr.ReviewedByUser)
            .FirstOrDefaultAsync(sr => sr.Id == command.SessionReportId, cancellationToken);

        if (sessionReport is null)
        {
            return Result.Failure<RejectSessionReportResponse>(
                SessionReportErrors.NotFound(command.SessionReportId));
        }

        // Only REVIEW status can be rejected
        if (sessionReport.Status != ReportStatus.Review)
        {
            return Result.Failure<RejectSessionReportResponse>(
                SessionReportErrors.InvalidStatusForOperation(
                    sessionReport.Status,
                    "reject"));
        }

        // Update status to Rejected
        sessionReport.Status = ReportStatus.Rejected;
        sessionReport.ReviewedByUserId = userContext.UserId;
        sessionReport.ReviewedAt = VietnamTime.UtcNow();
        sessionReport.UpdatedAt = VietnamTime.UtcNow();

        await ReportRequestWorkflow.MarkSessionRequestReviewedAsync(
            context,
            sessionReport.Id,
            ReportRequestStatus.Rejected,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new RejectSessionReportResponse
        {
            Id = sessionReport.Id,
            SessionId = sessionReport.SessionId,
            StudentProfileId = sessionReport.StudentProfileId,
            StudentName = sessionReport.StudentProfile.DisplayName,
            TeacherUserId = sessionReport.TeacherUserId,
            TeacherName = sessionReport.TeacherUser.Name,
            ReportDate = sessionReport.ReportDate,
            Feedback = sessionReport.Feedback,
            Status = sessionReport.Status,
            ReviewedByUserId = sessionReport.ReviewedByUserId,
            ReviewedByUserName = sessionReport.ReviewedByUser?.Name,
            CreatedAt = sessionReport.CreatedAt,
            UpdatedAt = sessionReport.UpdatedAt
        };
    }
}
