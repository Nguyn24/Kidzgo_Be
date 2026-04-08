using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Reports;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;

namespace Kidzgo.Application.MonthlyReports.GenerateMonthlyReportDraft;

/// <summary>
/// UC-176: Generate draft Monthly Report bằng AI
/// </summary>
public sealed class GenerateMonthlyReportDraftCommandHandler(
    IDbContext context,
    IUserContext userContext,
    IMonthlyReportDataAggregator dataAggregator,
    IAiReportGenerator aiReportGenerator
) : ICommandHandler<GenerateMonthlyReportDraftCommand, GenerateMonthlyReportDraftResponse>
{
    public async Task<Result<GenerateMonthlyReportDraftResponse>> Handle(
        GenerateMonthlyReportDraftCommand command,
        CancellationToken cancellationToken)
    {
        // Get report
        var report = await context.StudentMonthlyReports
            .Include(r => r.StudentProfile)
            .Include(r => r.Class)
            .FirstOrDefaultAsync(r => r.Id == command.ReportId, cancellationToken);

        if (report is null)
        {
            return Result.Failure<GenerateMonthlyReportDraftResponse>(
                MonthlyReportErrors.NotFound(command.ReportId));
        }

        // Authorization: Only teachers can generate drafts
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null || currentUser.Role != Domain.Users.UserRole.Teacher)
        {
            return Result.Failure<GenerateMonthlyReportDraftResponse>(
                Error.Validation("MonthlyReport.Unauthorized", "Only teachers can generate report drafts"));
        }

        // Check if teacher is assigned to this class
        var isTeacherOfClass = await context.Classes
            .AnyAsync(c => c.Id == report.ClassId &&
                         (c.MainTeacherId == currentUser.Id || c.AssistantTeacherId == currentUser.Id),
                cancellationToken);

        if (!isTeacherOfClass)
        {
            return Result.Failure<GenerateMonthlyReportDraftResponse>(
                Error.Validation("MonthlyReport.Unauthorized", "You can only generate drafts for reports of your classes"));
        }

        // Get or create MonthlyReportData
        var reportData = await context.MonthlyReportData
            .FirstOrDefaultAsync(rd => rd.ReportId == report.Id, cancellationToken);

        // Always refresh aggregated data so the draft reflects the latest class-scoped data.
        var aggregatedDataJson = await dataAggregator.AggregateDataAsync(
            report.StudentProfileId,
            report.ClassId,
            report.Month,
            report.Year,
            cancellationToken);

        if (reportData is null)
        {
            reportData = new MonthlyReportData
            {
                Id = Guid.NewGuid(),
                ReportId = report.Id,
                StudentProfileId = report.StudentProfileId,
                Month = report.Month,
                Year = report.Year,
                CreatedAt = VietnamTime.UtcNow(),
                UpdatedAt = VietnamTime.UtcNow()
            };

            context.MonthlyReportData.Add(reportData);
        }
        else
        {
            reportData.UpdatedAt = VietnamTime.UtcNow();
        }

        using var doc = JsonDocument.Parse(aggregatedDataJson);
        var root = doc.RootElement;

        reportData.AttendanceData = root.TryGetProperty("attendance", out var attendance)
            ? attendance.GetRawText()
            : null;
        reportData.HomeworkData = root.TryGetProperty("homework", out var homework)
            ? homework.GetRawText()
            : null;
        reportData.TestData = root.TryGetProperty("test", out var test)
            ? test.GetRawText()
            : null;
        reportData.MissionData = root.TryGetProperty("mission", out var mission)
            ? mission.GetRawText()
            : null;
        reportData.NotesData = root.TryGetProperty("notes", out var notes)
            ? notes.GetRawText()
            : null;
        reportData.TopicsData = root.TryGetProperty("topics", out var topics)
            ? topics.GetRawText()
            : null;

        // Generate draft using AI
        try
        {
            if (string.IsNullOrWhiteSpace(aggregatedDataJson))
            {
                return Result.Failure<GenerateMonthlyReportDraftResponse>(
                    Error.Validation("MonthlyReport.NoData", 
                        "No aggregated data available. Please aggregate data first."));
            }

            var draftContent = await aiReportGenerator.GenerateDraftAsync(
                aggregatedDataJson,
                report.StudentProfileId,
                report.ClassId,
                report.Month,
                report.Year,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(draftContent))
            {
                return Result.Failure<GenerateMonthlyReportDraftResponse>(
                    Error.Failure("MonthlyReport.EmptyDraft", 
                        "AI service returned empty draft content"));
            }

            // Update report
            report.DraftContent = draftContent;
            report.Status = ReportStatus.Draft; // Ensure status is Draft
            report.UpdatedAt = VietnamTime.UtcNow();

            await context.SaveChangesAsync(cancellationToken);

            return new GenerateMonthlyReportDraftResponse(
                report.Id,
                report.DraftContent,
                report.UpdatedAt);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<GenerateMonthlyReportDraftResponse>(
                Error.Failure("MonthlyReport.AiGenerationFailed", ex.Message));
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<GenerateMonthlyReportDraftResponse>(
                Error.Failure("MonthlyReport.AiServiceUnavailable", 
                    $"AI service is not available: {ex.Message}. Please ensure AI-KidzGo service is running."));
        }
        catch (Exception ex)
        {
            return Result.Failure<GenerateMonthlyReportDraftResponse>(
                Error.Failure("MonthlyReport.AiGenerationFailed", 
                    $"Failed to generate draft using AI: {ex.Message}"));
        }
    }
}

