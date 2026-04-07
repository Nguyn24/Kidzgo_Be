using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportRequests.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MonthlyReports.RejectMonthlyReport;

/// <summary>
/// UC-184: Staff/Admin reject Monthly Report
/// </summary>
public sealed class RejectMonthlyReportCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<RejectMonthlyReportCommand, RejectMonthlyReportResponse>
{
    public async Task<Result<RejectMonthlyReportResponse>> Handle(
        RejectMonthlyReportCommand command,
        CancellationToken cancellationToken)
    {
        var report = await context.StudentMonthlyReports
            .FirstOrDefaultAsync(r => r.Id == command.ReportId, cancellationToken);

        if (report is null)
        {
            return Result.Failure<RejectMonthlyReportResponse>(
                MonthlyReportErrors.NotFound(command.ReportId));
        }

        // Validate: Only can reject when status is Review
        if (report.Status != ReportStatus.Review)
        {
            return Result.Failure<RejectMonthlyReportResponse>(
                MonthlyReportErrors.InvalidStatus(report.Status, "reject"));
        }

        var reviewedBy = userContext.UserId;
        var now = DateTime.UtcNow;

        report.Status = ReportStatus.Rejected;
        report.ReviewedBy = reviewedBy;
        report.ReviewedAt = now;
        report.UpdatedAt = now;

        await ReportRequestWorkflow.MarkMonthlyRequestReviewedAsync(
            context,
            report.Id,
            ReportRequestStatus.Rejected,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new RejectMonthlyReportResponse
        {
            Id = report.Id,
            Status = report.Status.ToString(),
            ReviewedBy = report.ReviewedBy,
            ReviewedAt = report.ReviewedAt,
            UpdatedAt = report.UpdatedAt
        };
    }
}

