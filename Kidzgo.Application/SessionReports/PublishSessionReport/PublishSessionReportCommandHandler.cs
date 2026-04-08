using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.SessionReports.PublishSessionReport;

public sealed class PublishSessionReportCommandHandler(
    IDbContext context
) : ICommandHandler<PublishSessionReportCommand, PublishSessionReportResponse>
{
    public async Task<Result<PublishSessionReportResponse>> Handle(
        PublishSessionReportCommand command,
        CancellationToken cancellationToken)
    {
        var sessionReport = await context.SessionReports
            .Include(sr => sr.StudentProfile)
            .FirstOrDefaultAsync(sr => sr.Id == command.SessionReportId, cancellationToken);

        if (sessionReport is null)
        {
            return Result.Failure<PublishSessionReportResponse>(
                SessionReportErrors.NotFound(command.SessionReportId));
        }

        // Only APPROVED status can be published
        if (sessionReport.Status != ReportStatus.Approved)
        {
            return Result.Failure<PublishSessionReportResponse>(
                SessionReportErrors.InvalidStatusForOperation(
                    sessionReport.Status,
                    "publish"));
        }

        // Update status to Published
        sessionReport.Status = ReportStatus.Published;
        sessionReport.PublishedAt = VietnamTime.UtcNow();
        sessionReport.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return new PublishSessionReportResponse
        {
            Id = sessionReport.Id,
            SessionId = sessionReport.SessionId,
            StudentProfileId = sessionReport.StudentProfileId,
            StudentName = sessionReport.StudentProfile.DisplayName,
            Status = sessionReport.Status,
            PublishedAt = sessionReport.PublishedAt,
            UpdatedAt = sessionReport.UpdatedAt
        };
    }
}
