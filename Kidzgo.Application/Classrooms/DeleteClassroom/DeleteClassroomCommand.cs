using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Classrooms.DeleteClassroom;

public sealed class DeleteClassroomCommand : ICommand
{
    public Guid Id { get; init; }
}

