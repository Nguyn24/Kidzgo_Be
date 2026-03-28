using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Missions.GetMyMissions;

public sealed class GetMyMissionsQuery : IQuery<GetMyMissionsResponse>, IPageableQuery
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
