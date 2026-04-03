using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Classrooms.UpdateClassroom;

public sealed class UpdateClassroomCommand : ICommand<UpdateClassroomResponse>
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public string Name { get; init; } = null!;
    public int Capacity { get; init; }
    public string? Note { get; init; }
    public string? Floor { get; init; }
    public decimal? Area { get; init; }
    public List<string>? Equipment { get; init; }
}

