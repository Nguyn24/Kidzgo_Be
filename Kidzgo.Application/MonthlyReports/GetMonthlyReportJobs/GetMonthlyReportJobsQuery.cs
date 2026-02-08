using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.MonthlyReports.GetMonthlyReportJobs;

/// <summary>
/// UC-177: Xem danh sách Monthly Report Jobs
/// </summary>
public sealed class GetMonthlyReportJobsQuery : IQuery<GetMonthlyReportJobsResponse>
{
    public Guid? BranchId { get; init; }
    public int? Month { get; init; }
    public int? Year { get; init; }
    public string? Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

