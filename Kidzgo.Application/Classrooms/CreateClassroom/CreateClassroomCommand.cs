using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Classrooms.CreateClassroom;

public sealed class CreateClassroomCommand : ICommand<CreateClassroomResponse>
{
    public Guid BranchId { get; init; }
    public string Name { get; init; } = null!;
    public int Capacity { get; init; }
    public string? Note { get; init; }
}

