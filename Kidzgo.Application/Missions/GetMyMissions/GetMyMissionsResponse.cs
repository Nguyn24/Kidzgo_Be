using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Missions.GetMyMissions;

public sealed class GetMyMissionsResponse
{
    public Guid StudentProfileId { get; init; }
    public Page<MyMissionProgressDto> Missions { get; init; } = null!;
}

public sealed class MyMissionProgressDto
{
    public Guid Id { get; init; }
    public Guid MissionId { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public string MissionType { get; init; } = null!;
    public string Status { get; init; } = null!;
    public decimal? ProgressValue { get; init; }
    public int? TotalRequired { get; init; }
    public decimal ProgressPercentage { get; init; }
    public int? RewardStars { get; init; }
    public int? RewardExp { get; init; }
    public DateTime? StartAt { get; init; }
    public DateTime? EndAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}
