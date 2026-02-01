using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Homework.DeleteHomeworkAssignment;

public sealed class DeleteHomeworkAssignmentCommand : ICommand
{
    public Guid Id { get; init; }
}

