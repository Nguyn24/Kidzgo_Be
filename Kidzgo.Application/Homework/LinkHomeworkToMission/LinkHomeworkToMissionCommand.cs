using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Homework.LinkHomeworkToMission;

public sealed class LinkHomeworkToMissionCommand : ICommand<LinkHomeworkToMissionResponse>
{
    public Guid HomeworkId { get; init; }
    public Guid MissionId { get; init; }
}

