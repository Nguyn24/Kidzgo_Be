namespace Kidzgo.Application.TeachingMaterials.UpdateTeachingMaterialViewProgress;

public sealed class TeachingMaterialViewProgressResponse
{
    public Guid MaterialId { get; init; }
    public Guid UserId { get; init; }
    public int ProgressPercent { get; init; }
    public int? LastSlideViewed { get; init; }
    public int TotalTimeSeconds { get; init; }
    public DateTime FirstViewedAt { get; init; }
    public DateTime LastViewedAt { get; init; }
    public int ViewCount { get; init; }
    public bool Completed { get; init; }
}
