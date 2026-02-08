using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Kidzgo.Domain.Reports.Events;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MonthlyReports.PublishMonthlyReport;

/// <summary>
/// UC-185: Publish Monthly Report
/// </summary>
public sealed class PublishMonthlyReportCommandHandler(
    IDbContext context
) : ICommandHandler<PublishMonthlyReportCommand, PublishMonthlyReportResponse>
{
    public async Task<Result<PublishMonthlyReportResponse>> Handle(
        PublishMonthlyReportCommand command,
        CancellationToken cancellationToken)
    {
        var report = await context.StudentMonthlyReports
            .FirstOrDefaultAsync(r => r.Id == command.ReportId, cancellationToken);

        if (report is null)
        {
            return Result.Failure<PublishMonthlyReportResponse>(
                MonthlyReportErrors.NotFound(command.ReportId));
        }

        // Validate: Only can publish when status is Approved
        if (report.Status != ReportStatus.Approved)
        {
            return Result.Failure<PublishMonthlyReportResponse>(
                MonthlyReportErrors.InvalidStatus(report.Status, "publish"));
        }

        var now = DateTime.UtcNow;

        report.Status = ReportStatus.Published;
        report.PublishedAt = now;
        report.UpdatedAt = now;

        // Copy DraftContent to FinalContent if FinalContent is empty
        if (string.IsNullOrWhiteSpace(report.FinalContent) && !string.IsNullOrWhiteSpace(report.DraftContent))
        {
            report.FinalContent = report.DraftContent;
        }

        // Raise domain event (will be published by infrastructure)
        report.Raise(new MonthlyReportPublishedEvent(
            report.Id,
            report.StudentProfileId,
            report.Month,
            report.Year));

        await context.SaveChangesAsync(cancellationToken);

        return new PublishMonthlyReportResponse
        {
            Id = report.Id,
            Status = report.Status.ToString(),
            PublishedAt = report.PublishedAt,
            UpdatedAt = report.UpdatedAt
        };
    }
}

