namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialViewProgressSummary;

public sealed class GetTeachingMaterialViewProgressSummaryResponse
{
    public Guid MaterialId { get; init; }
    public int TotalViewers { get; init; }
    public int CompletedCount { get; init; }
    public decimal AverageProgressPercent { get; init; }
    public decimal AverageTimeSeconds { get; init; }
    public IReadOnlyCollection<TeachingMaterialProgressViewerDto> Viewers { get; init; } = [];
}

public sealed class TeachingMaterialProgressViewerDto
{
    public Guid UserId { get; init; }
    public string? UserName { get; init; }
    public string? AvatarUrl { get; init; }
    public int ProgressPercent { get; init; }
    public int? LastSlideViewed { get; init; }
    public int TotalTimeSeconds { get; init; }
    public int ViewCount { get; init; }
    public bool Completed { get; init; }
    public DateTime LastViewedAt { get; init; }
}
