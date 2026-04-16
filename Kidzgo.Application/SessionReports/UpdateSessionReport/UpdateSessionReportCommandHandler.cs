using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.SessionReports;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.SessionReports.UpdateSessionReport;

public sealed class UpdateSessionReportCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateSessionReportCommand, UpdateSessionReportResponse>
{
    public async Task<Result<UpdateSessionReportResponse>> Handle(
        UpdateSessionReportCommand command,
        CancellationToken cancellationToken)
    {
        var sessionReport = await context.SessionReports
            .Include(sr => sr.Session)
                .ThenInclude(s => s.Class)
            .Include(sr => sr.StudentProfile)
            .Include(sr => sr.TeacherUser)
            .FirstOrDefaultAsync(sr => sr.Id == command.Id, cancellationToken);

        if (sessionReport is null)
        {
            return Result.Failure<UpdateSessionReportResponse>(SessionReportErrors.NotFound(command.Id));
        }

        // Validate: Can edit when status is Draft, Review, or Rejected
        // Rejected reports can be edited to allow teacher to fix and resubmit
        if (sessionReport.Status != ReportStatus.Draft &&
            sessionReport.Status != ReportStatus.Review &&
            sessionReport.Status != ReportStatus.Rejected)
        {
            return Result.Failure<UpdateSessionReportResponse>(
                SessionReportErrors.InvalidStatusForOperation(sessionReport.Status, "edit"));
        }

        var now = VietnamTime.UtcNow();
        var sessionEndedCheck = SessionReportScheduleGuard.EnsureSessionHasEnded(sessionReport.Session, now);
        if (sessionEndedCheck.IsFailure)
        {
            return Result.Failure<UpdateSessionReportResponse>(sessionEndedCheck.Error);
        }

        // Update feedback
        sessionReport.Feedback = command.Feedback;
        sessionReport.UpdatedAt = now;

        // If report was Rejected, change status back to Draft so teacher can resubmit
        if (sessionReport.Status == ReportStatus.Rejected)
        {
            sessionReport.Status = ReportStatus.Draft;
            // Clear review information since it's being edited again
            sessionReport.ReviewedByUserId = null;
            sessionReport.ReviewedAt = null;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateSessionReportResponse
        {
            Id = sessionReport.Id,
            SessionId = sessionReport.SessionId,
            SessionDate = sessionReport.Session.PlannedDatetime,
            StudentProfileId = sessionReport.StudentProfileId,
            StudentName = sessionReport.StudentProfile.DisplayName,
            TeacherUserId = sessionReport.TeacherUserId,
            TeacherName = sessionReport.TeacherUser.Name,
            ReportDate = sessionReport.ReportDate,
            Feedback = sessionReport.Feedback,
            IsMonthlyCompiled = sessionReport.IsMonthlyCompiled,
            UpdatedAt = sessionReport.UpdatedAt
        };
    }
}

