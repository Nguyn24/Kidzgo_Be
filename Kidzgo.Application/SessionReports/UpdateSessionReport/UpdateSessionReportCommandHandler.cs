using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
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

        // Update feedback
        sessionReport.Feedback = command.Feedback;
        sessionReport.UpdatedAt = DateTime.UtcNow;

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

