namespace Kidzgo.Application.Sessions.GetTeacherTimetable;

public sealed class GetTeacherTimetableResponse
{
    public List<TimetableItemDto> Sessions { get; init; } = new();
}

public sealed class TimetableItemDto
{
    public Guid Id { get; init; }
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public DateTime PlannedDatetime { get; init; }
    public DateTime? ActualDatetime { get; init; }
    public int DurationMinutes { get; init; }
    public Domain.Sessions.ParticipationType ParticipationType { get; init; }
    public Domain.Sessions.SessionStatus Status { get; init; }
    public Guid? PlannedRoomId { get; init; }
    public string? PlannedRoomName { get; init; }
    public Guid? ActualRoomId { get; init; }
    public string? ActualRoomName { get; init; }
    public Guid? PlannedTeacherId { get; init; }
    public string? PlannedTeacherName { get; init; }
    public Guid? ActualTeacherId { get; init; }
    public string? ActualTeacherName { get; init; }
    public Guid? PlannedAssistantId { get; init; }
    public string? PlannedAssistantName { get; init; }
    public Guid? LessonPlanId { get; init; }
    public string? LessonPlanLink { get; init; }
}

