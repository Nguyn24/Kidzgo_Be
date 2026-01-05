namespace Kidzgo.Application.Classrooms.UpdateClassroom;

public sealed class UpdateClassroomResponse
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public string Name { get; init; } = null!;
    public int Capacity { get; init; }
    public string? Note { get; init; }
    public bool IsActive { get; init; }
}

