namespace Kidzgo.API.Requests;

public sealed class AssignTeacherRequest
{
    public Guid? MainTeacherId { get; set; }
    public Guid? AssistantTeacherId { get; set; }
}

