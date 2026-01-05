namespace Kidzgo.Application.Classes.AssignTeacher;

public sealed class AssignTeacherResponse
{
    public Guid ClassId { get; init; }
    public Guid? MainTeacherId { get; init; }
    public string? MainTeacherName { get; init; }
    public Guid? AssistantTeacherId { get; init; }
    public string? AssistantTeacherName { get; init; }
}

