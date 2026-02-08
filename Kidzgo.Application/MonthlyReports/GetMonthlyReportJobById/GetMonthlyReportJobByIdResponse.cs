namespace Kidzgo.Application.MonthlyReports.GetMonthlyReportJobById;

public sealed class GetMonthlyReportJobByIdResponse
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
    public DateTime UpdatedAt { get; init; }
    public List<ReportSummaryDto> Reports { get; init; } = new();
}

public sealed class ReportSummaryDto
{
    public Guid Id { get; init; }
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public Guid? ClassId { get; init; }
    public string? ClassName { get; init; }
    public string Status { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}

