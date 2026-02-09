using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Reports;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Application.MonthlyReports.GenerateMonthlyReportPdf;

/// <summary>
/// Generate or regenerate PDF for a Monthly Report (on-demand)
/// </summary>
public sealed class GenerateMonthlyReportPdfCommandHandler(
    IDbContext context,
    IPdfReportGenerator pdfGenerator,
    IFileStorageService fileStorage,
    ILogger<GenerateMonthlyReportPdfCommandHandler> logger
) : ICommandHandler<GenerateMonthlyReportPdfCommand, GenerateMonthlyReportPdfResponse>
{
    public async Task<Result<GenerateMonthlyReportPdfResponse>> Handle(
        GenerateMonthlyReportPdfCommand command,
        CancellationToken cancellationToken)
    {
        var report = await context.StudentMonthlyReports
            .Include(r => r.StudentProfile)
            .FirstOrDefaultAsync(r => r.Id == command.ReportId, cancellationToken);

        if (report is null)
        {
            return Result.Failure<GenerateMonthlyReportPdfResponse>(
                MonthlyReportErrors.NotFound(command.ReportId));
        }

        // Validate: Only can generate PDF for reports that have content
        var contentToGenerate = report.FinalContent ?? report.DraftContent;
        if (string.IsNullOrWhiteSpace(contentToGenerate))
        {
            return Result.Failure<GenerateMonthlyReportPdfResponse>(
                Error.Validation("MonthlyReport.NoContent", 
                    "Cannot generate PDF: Report has no content. Please add draft or final content first."));
        }

        // Validate: Only can generate PDF for published reports or reports in review/approved status
        // (Allow regenerate for any status that has content)
        if (report.Status == ReportStatus.Draft && string.IsNullOrWhiteSpace(report.DraftContent))
        {
            return Result.Failure<GenerateMonthlyReportPdfResponse>(
                Error.Validation("MonthlyReport.InvalidStatus",
                    "Cannot generate PDF for draft reports without content."));
        }

        try
        {
            var studentName = report.StudentProfile.DisplayName;
            var pdfUrl = await pdfGenerator.GeneratePdfAsync(
                contentToGenerate,
                studentName,
                report.Month,
                report.Year,
                cancellationToken);

            var now = DateTime.UtcNow;

            // Store original URL without fl_attachment in database
            // fl_attachment will be added when returning in response
            report.PdfUrl = pdfUrl;
            report.PdfGeneratedAt = now;
            report.UpdatedAt = now;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "PDF generated successfully for report {ReportId}, Student: {StudentName}, URL: {PdfUrl}",
                report.Id, studentName, pdfUrl);

            // Get download URL with force download flag for response
            // Note: pdfUrl is stored without fl_attachment, we add it here for response
            string downloadUrl;
            try
            {
                downloadUrl = fileStorage.GetDownloadUrl(pdfUrl);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to generate download URL for PDF, using original URL");
                downloadUrl = pdfUrl;
            }

            return new GenerateMonthlyReportPdfResponse
            {
                ReportId = report.Id,
                PdfUrl = downloadUrl,
                PdfGeneratedAt = now
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to generate PDF for report {ReportId}",
                report.Id);
            
            return Result.Failure<GenerateMonthlyReportPdfResponse>(
                Error.Failure("MonthlyReport.PdfGenerationFailed",
                    $"Failed to generate PDF: {ex.Message}"));
        }
    }
}

