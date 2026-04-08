using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportRequests.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MonthlyReports.SubmitMonthlyReport;

/// <summary>
/// UC-181: Teacher submit Monthly Report
/// </summary>
public sealed class SubmitMonthlyReportCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<SubmitMonthlyReportCommand, SubmitMonthlyReportResponse>
{
    public async Task<Result<SubmitMonthlyReportResponse>> Handle(
        SubmitMonthlyReportCommand command,
        CancellationToken cancellationToken)
    {
        var report = await context.StudentMonthlyReports
            .FirstOrDefaultAsync(r => r.Id == command.ReportId, cancellationToken);

        if (report is null)
        {
            return Result.Failure<SubmitMonthlyReportResponse>(
                MonthlyReportErrors.NotFound(command.ReportId));
        }

        // Validate: Only can submit when status is Draft
        if (report.Status != ReportStatus.Draft)
        {
            return Result.Failure<SubmitMonthlyReportResponse>(
                MonthlyReportErrors.InvalidStatus(report.Status, "submit"));
        }

        var submittedBy = userContext.UserId;
        var now = VietnamTime.UtcNow();

        report.Status = ReportStatus.Review;
        report.SubmittedBy = submittedBy;
        report.UpdatedAt = now;

        await ReportRequestWorkflow.MarkMatchingMonthlyRequestSubmittedAsync(
            context,
            report,
            submittedBy,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new SubmitMonthlyReportResponse
        {
            Id = report.Id,
            Status = report.Status.ToString(),
            SubmittedBy = report.SubmittedBy,
            UpdatedAt = report.UpdatedAt
        };
    }
}

