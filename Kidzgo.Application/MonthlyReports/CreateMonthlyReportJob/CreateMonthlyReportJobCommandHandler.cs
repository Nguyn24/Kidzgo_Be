using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Kidzgo.Domain.Schools.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MonthlyReports.CreateMonthlyReportJob;

/// <summary>
/// UC-174: Tạo Monthly Report Job
/// </summary>
public sealed class CreateMonthlyReportJobCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateMonthlyReportJobCommand, CreateMonthlyReportJobResponse>
{
    public async Task<Result<CreateMonthlyReportJobResponse>> Handle(
        CreateMonthlyReportJobCommand command,
        CancellationToken cancellationToken)
    {
        // Validate month and year
        if (command.Month < 1 || command.Month > 12)
        {
            return Result.Failure<CreateMonthlyReportJobResponse>(
                Error.Validation("MonthlyReport.InvalidMonth", "Month must be between 1 and 12"));
        }

        if (command.Year < 2000 || command.Year > 2100)
        {
            return Result.Failure<CreateMonthlyReportJobResponse>(
                Error.Validation("MonthlyReport.InvalidYear", "Year must be between 2000 and 2100"));
        }

        // Verify branch exists
        var branch = await context.Branches
            .FirstOrDefaultAsync(b => b.Id == command.BranchId, cancellationToken);

        if (branch is null)
        {
            return Result.Failure<CreateMonthlyReportJobResponse>(
                BranchErrors.NotFound(command.BranchId));
        }

        var now = DateTime.UtcNow;
        var createdBy = userContext.UserId;

        var job = new MonthlyReportJob
        {
            Id = Guid.NewGuid(),
            Month = command.Month,
            Year = command.Year,
            BranchId = command.BranchId,
            Status = MonthlyReportJobStatus.Pending,
            CreatedBy = createdBy,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.MonthlyReportJobs.Add(job);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateMonthlyReportJobResponse
        {
            Id = job.Id,
            Month = job.Month,
            Year = job.Year,
            BranchId = job.BranchId,
            Status = job.Status.ToString(),
            CreatedBy = job.CreatedBy,
            CreatedAt = job.CreatedAt
        };
    }
}

