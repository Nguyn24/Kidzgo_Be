using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Missions.GetMissionProgress;

public sealed class GetMissionProgressResponse
{
    public MissionProgressInfoDto Mission { get; init; } = null!;
    public Page<MissionProgressDto> Progresses { get; init; } = null!;
}

public sealed class MissionProgressInfoDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
}

public sealed class MissionProgressDto
{
    public Guid Id { get; init; }
    public Guid MissionId { get; init; }
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public string Status { get; init; } = null!;
    public decimal? ProgressValue { get; init; }
    public decimal ProgressPercentage { get; init; }
    public DateTime? CompletedAt { get; init; }
    public Guid? VerifiedBy { get; init; }
    public string? VerifiedByName { get; init; }
}

