using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Classes.AssignTeacher;

public sealed class AssignTeacherCommand : ICommand<AssignTeacherResponse>
{
    public Guid ClassId { get; init; }
    public Guid? MainTeacherId { get; init; }
    public Guid? AssistantTeacherId { get; init; }
}

