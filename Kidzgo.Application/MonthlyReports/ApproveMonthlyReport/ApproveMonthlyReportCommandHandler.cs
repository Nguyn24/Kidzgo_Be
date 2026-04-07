using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportRequests.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MonthlyReports.ApproveMonthlyReport;

/// <summary>
/// UC-183: Staff/Admin approve Monthly Report
/// </summary>
public sealed class ApproveMonthlyReportCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<ApproveMonthlyReportCommand, ApproveMonthlyReportResponse>
{
    public async Task<Result<ApproveMonthlyReportResponse>> Handle(
        ApproveMonthlyReportCommand command,
        CancellationToken cancellationToken)
    {
        var report = await context.StudentMonthlyReports
            .FirstOrDefaultAsync(r => r.Id == command.ReportId, cancellationToken);

        if (report is null)
        {
            return Result.Failure<ApproveMonthlyReportResponse>(
                MonthlyReportErrors.NotFound(command.ReportId));
        }

        // Validate: Only can approve when status is Review
        if (report.Status != ReportStatus.Review)
        {
            return Result.Failure<ApproveMonthlyReportResponse>(
                MonthlyReportErrors.InvalidStatus(report.Status, "approve"));
        }

        var reviewedBy = userContext.UserId;
        var now = DateTime.UtcNow;

        report.Status = ReportStatus.Approved;
        report.ReviewedBy = reviewedBy;
        report.ReviewedAt = now;
        report.UpdatedAt = now;

        await ReportRequestWorkflow.MarkMonthlyRequestReviewedAsync(
            context,
            report.Id,
            ReportRequestStatus.Approved,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new ApproveMonthlyReportResponse
        {
            Id = report.Id,
            Status = report.Status.ToString(),
            ReviewedBy = report.ReviewedBy,
            ReviewedAt = report.ReviewedAt,
            UpdatedAt = report.UpdatedAt
        };
    }
}

