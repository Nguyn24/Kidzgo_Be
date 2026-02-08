using Kidzgo.Domain.Common;

namespace Kidzgo.Application.MonthlyReports.GetMonthlyReportJobs;

public sealed class GetMonthlyReportJobsResponse
{
    public Page<MonthlyReportJobDto> Jobs { get; init; } = null!;
}

public sealed class MonthlyReportJobDto
{
    public Guid Id { get; init; }
    public int Month { get; init; }
    public int Year { get; init; }
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public string Status { get; init; } = null!;
    public DateTime? StartedAt { get; init; }
    public DateTime? FinishedAt { get; init; }
    public string? ErrorMessage { get; init; }
    public int RetryCount { get; init; }
    public Guid? CreatedBy { get; init; }
    public string? CreatedByName { get; init; }
    public DateTime CreatedAt { get; init; }
    public int ReportCount { get; init; }
}

