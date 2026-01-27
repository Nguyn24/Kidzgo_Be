using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Missions.DeleteMission;

public sealed class DeleteMissionCommand : ICommand
{
    public Guid Id { get; init; }
}

