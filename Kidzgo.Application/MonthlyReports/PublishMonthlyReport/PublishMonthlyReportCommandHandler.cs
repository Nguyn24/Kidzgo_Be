using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Kidzgo.Domain.Reports.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Application.MonthlyReports.PublishMonthlyReport;

/// <summary>
/// UC-185: Publish Monthly Report
/// </summary>
public sealed class PublishMonthlyReportCommandHandler(
    IDbContext context,
    IFileStorageService fileStorage,
    ILogger<PublishMonthlyReportCommandHandler> logger
) : ICommandHandler<PublishMonthlyReportCommand, PublishMonthlyReportResponse>
{
    public async Task<Result<PublishMonthlyReportResponse>> Handle(
        PublishMonthlyReportCommand command,
        CancellationToken cancellationToken)
    {
        var report = await context.StudentMonthlyReports
            .Include(r => r.StudentProfile)
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

        // Get download URL with force download flag if PDF exists
        // Note: report.PdfUrl is stored without fl_attachment, we add it here for response
        string? downloadUrl = null;
        if (!string.IsNullOrWhiteSpace(report.PdfUrl))
        {
            try
            {
                downloadUrl = fileStorage.GetDownloadUrl(report.PdfUrl);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to generate download URL for PDF, using original URL");
                downloadUrl = report.PdfUrl;
            }
        }

        return new PublishMonthlyReportResponse
        {
            Id = report.Id,
            Status = report.Status.ToString(),
            PublishedAt = report.PublishedAt,
            UpdatedAt = report.UpdatedAt,
            PdfUrl = downloadUrl
        };
    }
}

