namespace Kidzgo.Application.IncidentReports.GetIncidentReportStatistics;

public sealed class GetIncidentReportStatisticsResponse
{
    public int Total { get; init; }
    public int Open { get; init; }
    public int InProgress { get; init; }
    public int Resolved { get; init; }
    public int Closed { get; init; }
    public int Rejected { get; init; }
    public int Unassigned { get; init; }
    public IReadOnlyList<IncidentStatusStatDto> ByStatus { get; init; } = Array.Empty<IncidentStatusStatDto>();
    public IReadOnlyList<IncidentCategoryStatDto> ByCategory { get; init; } = Array.Empty<IncidentCategoryStatDto>();
}

public sealed class IncidentStatusStatDto
{
    public string Status { get; init; } = null!;
    public int Count { get; init; }
}

public sealed class IncidentCategoryStatDto
{
    public string Category { get; init; } = null!;
    public int Count { get; init; }
}
