using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportRequests.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.SessionReports.SubmitSessionReport;

public sealed class SubmitSessionReportCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<SubmitSessionReportCommand, SubmitSessionReportResponse>
{
    public async Task<Result<SubmitSessionReportResponse>> Handle(
        SubmitSessionReportCommand command,
        CancellationToken cancellationToken)
    {
        var sessionReport = await context.SessionReports
            .Include(sr => sr.Session)
            .Include(sr => sr.StudentProfile)
            .Include(sr => sr.TeacherUser)
            .Include(sr => sr.SubmittedByUser)
            .FirstOrDefaultAsync(sr => sr.Id == command.SessionReportId, cancellationToken);

        if (sessionReport is null)
        {
            return Result.Failure<SubmitSessionReportResponse>(
                SessionReportErrors.NotFound(command.SessionReportId));
        }

        // Only DRAFT status can be submitted
        if (sessionReport.Status != ReportStatus.Draft)
        {
            return Result.Failure<SubmitSessionReportResponse>(
                SessionReportErrors.InvalidStatusForOperation(
                    sessionReport.Status,
                    "submit"));
        }

        // Update status to Review
        sessionReport.Status = ReportStatus.Review;
        sessionReport.SubmittedByUserId = userContext.UserId;
        sessionReport.UpdatedAt = VietnamTime.UtcNow();

        await ReportRequestWorkflow.MarkMatchingSessionRequestSubmittedAsync(
            context,
            sessionReport,
            sessionReport.Session.ClassId,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new SubmitSessionReportResponse
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
            SubmittedByUserId = sessionReport.SubmittedByUserId,
            SubmittedByName = sessionReport.SubmittedByUser?.Name,
            CreatedAt = sessionReport.CreatedAt,
            UpdatedAt = sessionReport.UpdatedAt
        };
    }
}
