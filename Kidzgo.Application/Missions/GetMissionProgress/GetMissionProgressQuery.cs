using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Missions.GetMissionProgress;

public sealed class GetMissionProgressQuery : IQuery<GetMissionProgressResponse>, IPageableQuery
{
    public Guid MissionId { get; init; }
    public Guid? StudentProfileId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

