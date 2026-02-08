using Kidzgo.Domain.Common;

namespace Kidzgo.Application.MonthlyReports.GetMonthlyReports;

public sealed class GetMonthlyReportsResponse
{
    public Page<MonthlyReportSummaryDto> Reports { get; init; } = null!;
}

public sealed class MonthlyReportSummaryDto
{
    public Guid Id { get; init; }
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public Guid? ClassId { get; init; }
    public string? ClassName { get; init; }
    public Guid? JobId { get; init; }
    public int Month { get; init; }
    public int Year { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? PublishedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

