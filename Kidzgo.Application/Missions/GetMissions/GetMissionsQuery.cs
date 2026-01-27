using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Gamification;

namespace Kidzgo.Application.Missions.GetMissions;

public sealed class GetMissionsQuery : IQuery<GetMissionsResponse>, IPageableQuery
{
    public MissionScope? Scope { get; init; }
    public Guid? TargetClassId { get; init; }
    public string? TargetGroup { get; init; }
    public MissionType? MissionType { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

