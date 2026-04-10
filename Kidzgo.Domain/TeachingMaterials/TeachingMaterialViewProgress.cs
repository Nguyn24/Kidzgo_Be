using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.TeachingMaterials;

public class TeachingMaterialViewProgress : Entity
{
    public Guid Id { get; set; }
    public Guid TeachingMaterialId { get; set; }
    public Guid UserId { get; set; }
    public int ProgressPercent { get; set; }
    public int? LastSlideViewed { get; set; }
    public int TotalTimeSeconds { get; set; }
    public int ViewCount { get; set; } = 1;
    public bool Completed { get; set; }
    public DateTime FirstViewedAt { get; set; }
    public DateTime LastViewedAt { get; set; }

    public TeachingMaterial TeachingMaterial { get; set; } = null!;
    public User User { get; set; } = null!;

    public void ApplyProgress(int progressPercent, int? lastSlideViewed, int totalTimeSeconds, DateTime viewedAt)
    {
        ProgressPercent = Math.Clamp(progressPercent, 0, 100);
        LastSlideViewed = lastSlideViewed;
        TotalTimeSeconds = Math.Max(0, totalTimeSeconds);
        LastViewedAt = viewedAt;
        Completed = ProgressPercent >= 100;
    }
}
