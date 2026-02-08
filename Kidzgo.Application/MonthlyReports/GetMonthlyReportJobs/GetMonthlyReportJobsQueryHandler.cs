using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MonthlyReports.GetMonthlyReportJobs;

/// <summary>
/// UC-177: Xem danh sách Monthly Report Jobs
/// </summary>
public sealed class GetMonthlyReportJobsQueryHandler(
    IDbContext context
) : IQueryHandler<GetMonthlyReportJobsQuery, GetMonthlyReportJobsResponse>
{
    public async Task<Result<GetMonthlyReportJobsResponse>> Handle(
        GetMonthlyReportJobsQuery query,
        CancellationToken cancellationToken)
    {
        var jobsQuery = context.MonthlyReportJobs
            .Include(j => j.Branch)
            .Include(j => j.CreatedByUser)
            .Include(j => j.Reports)
            .AsQueryable();

        // Apply filters
        if (query.BranchId.HasValue)
        {
            jobsQuery = jobsQuery.Where(j => j.BranchId == query.BranchId.Value);
        }

        if (query.Month.HasValue)
        {
            jobsQuery = jobsQuery.Where(j => j.Month == query.Month.Value);
        }

        if (query.Year.HasValue)
        {
            jobsQuery = jobsQuery.Where(j => j.Year == query.Year.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (Enum.TryParse<MonthlyReportJobStatus>(query.Status, true, out var status))
            {
                jobsQuery = jobsQuery.Where(j => j.Status == status);
            }
        }

        // Get total count before pagination
        int totalCount = await jobsQuery.CountAsync(cancellationToken);

        // Apply pagination
        var jobs = await jobsQuery
            .OrderByDescending(j => j.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .ToListAsync(cancellationToken);

        var jobDtos = jobs.Select(j => new MonthlyReportJobDto
        {
            Id = j.Id,
            Month = j.Month,
            Year = j.Year,
            BranchId = j.BranchId,
            BranchName = j.Branch.Name,
            Status = j.Status.ToString(),
            StartedAt = j.StartedAt,
            FinishedAt = j.FinishedAt,
            ErrorMessage = j.ErrorMessage,
            RetryCount = j.RetryCount,
            CreatedBy = j.CreatedBy,
            CreatedByName = j.CreatedByUser?.Name,
            CreatedAt = j.CreatedAt,
            ReportCount = j.Reports.Count
        }).ToList();

        var page = new Page<MonthlyReportJobDto>(
            jobDtos,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetMonthlyReportJobsResponse
        {
            Jobs = page
        };
    }
}

