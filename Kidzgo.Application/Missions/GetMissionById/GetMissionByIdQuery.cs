using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Missions.GetMissionById;

public sealed class GetMissionByIdQuery : IQuery<GetMissionByIdResponse>
{
    public Guid Id { get; init; }
}

