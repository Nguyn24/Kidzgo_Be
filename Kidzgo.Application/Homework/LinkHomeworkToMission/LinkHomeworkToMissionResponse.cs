namespace Kidzgo.Application.Homework.LinkHomeworkToMission;

public sealed class LinkHomeworkToMissionResponse
{
    public Guid HomeworkId { get; init; }
    public Guid MissionId { get; init; }
    public string MissionTitle { get; init; } = null!;
}

