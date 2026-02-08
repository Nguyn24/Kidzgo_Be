using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kidzgo.Application.MonthlyReports.UpdateMonthlyReportDraft;

/// <summary>
/// UC-180: Teacher chỉnh sửa draft Monthly Report
/// </summary>
public sealed class UpdateMonthlyReportDraftCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<UpdateMonthlyReportDraftCommand, UpdateMonthlyReportDraftResponse>
{
    public async Task<Result<UpdateMonthlyReportDraftResponse>> Handle(
        UpdateMonthlyReportDraftCommand command,
        CancellationToken cancellationToken)
    {
        var report = await context.StudentMonthlyReports
            .Include(r => r.Class)
            .FirstOrDefaultAsync(r => r.Id == command.ReportId, cancellationToken);

        if (report is null)
        {
            return Result.Failure<UpdateMonthlyReportDraftResponse>(
                MonthlyReportErrors.NotFound(command.ReportId));
        }

        // Validate: Can edit when status is Draft, Review, or Rejected
        // Rejected reports can be edited to allow teacher to fix and resubmit
        if (report.Status != ReportStatus.Draft && 
            report.Status != ReportStatus.Review && 
            report.Status != ReportStatus.Rejected)
        {
            return Result.Failure<UpdateMonthlyReportDraftResponse>(
                MonthlyReportErrors.InvalidStatus(report.Status, "edit"));
        }

        // Authorization: Teacher can only edit reports of their classes
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null || currentUser.Role != Domain.Users.UserRole.Teacher)
        {
            return Result.Failure<UpdateMonthlyReportDraftResponse>(
                Error.Validation("MonthlyReport.Unauthorized", "Only teachers can edit reports"));
        }

        var isTeacherOfClass = await context.Classes
            .AnyAsync(c => c.Id == report.ClassId &&
                         (c.MainTeacherId == currentUser.Id || c.AssistantTeacherId == currentUser.Id),
                cancellationToken);

        if (!isTeacherOfClass)
        {
            return Result.Failure<UpdateMonthlyReportDraftResponse>(
                Error.Validation("MonthlyReport.Unauthorized", "You can only edit reports of your classes"));
        }

        // Validate and ensure DraftContent is valid JSON
        // If it's already valid JSON, use it; otherwise, wrap it as a JSON string
        string? validJsonContent = null;
        if (!string.IsNullOrWhiteSpace(command.DraftContent))
        {
            // Try to parse as JSON to validate
            try
            {
                JsonDocument.Parse(command.DraftContent);
                // If parsing succeeds, it's valid JSON
                validJsonContent = command.DraftContent;
            }
            catch (JsonException)
            {
                // If parsing fails, wrap the string as a JSON string value
                validJsonContent = JsonSerializer.Serialize(command.DraftContent);
            }
        }

        report.DraftContent = validJsonContent;
        report.UpdatedAt = DateTime.UtcNow;

        // If report was Rejected, change status back to Draft so teacher can resubmit
        if (report.Status == ReportStatus.Rejected)
        {
            report.Status = ReportStatus.Draft;
            // Clear review information since it's being edited again
            report.ReviewedBy = null;
            report.ReviewedAt = null;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateMonthlyReportDraftResponse
        {
            Id = report.Id,
            DraftContent = report.DraftContent,
            UpdatedAt = report.UpdatedAt
        };
    }
}

