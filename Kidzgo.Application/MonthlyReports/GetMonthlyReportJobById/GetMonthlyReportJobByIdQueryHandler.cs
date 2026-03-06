using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MonthlyReports.GetMonthlyReportJobById;

/// <summary>
/// UC-178: Xem trạng thái Monthly Report Job
/// </summary>
public sealed class GetMonthlyReportJobByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetMonthlyReportJobByIdQuery, GetMonthlyReportJobByIdResponse>
{
    public async Task<Result<GetMonthlyReportJobByIdResponse>> Handle(
        GetMonthlyReportJobByIdQuery query,
        CancellationToken cancellationToken)
    {
        var job = await context.MonthlyReportJobs
            .Include(j => j.Branch)
            .Include(j => j.CreatedByUser)
            .Include(j => j.Reports)
                .ThenInclude(r => r.StudentProfile)
            .Include(j => j.Reports)
                .ThenInclude(r => r.Class)
            .FirstOrDefaultAsync(j => j.Id == query.JobId, cancellationToken);

        if (job is null)
        {
            return Result.Failure<GetMonthlyReportJobByIdResponse>(
                MonthlyReportErrors.JobNotFound(query.JobId));
        }

        var reports = job.Reports.Select(r => new ReportSummaryDto
        {
            Id = r.Id,
            StudentProfileId = r.StudentProfileId,
            StudentName = r.StudentProfile.DisplayName,
            ClassId = r.ClassId,
            ClassName = r.Class?.Title,
            Status = r.Status.ToString(),
            CreatedAt = r.CreatedAt
        }).ToList();

        return new GetMonthlyReportJobByIdResponse
        {
            Id = job.Id,
            Month = job.Month,
            Year = job.Year,
            BranchId = job.BranchId,
            BranchName = job.Branch.Name,
            Status = job.Status.ToString(),
            StartedAt = job.StartedAt,
            FinishedAt = job.FinishedAt,
            ErrorMessage = job.ErrorMessage,
            RetryCount = job.RetryCount,
            CreatedBy = job.CreatedBy,
            CreatedByName = job.CreatedByUser?.Name,
            CreatedAt = job.CreatedAt,
            UpdatedAt = job.UpdatedAt,
            ReportCount = job.Reports.Count,
            Reports = reports
        };
    }
}

