namespace Kidzgo.Application.Classrooms.CreateClassroom;

public sealed class CreateClassroomResponse
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public string Name { get; init; } = null!;
    public int Capacity { get; init; }
    public string? Note { get; init; }
    public string? Floor { get; init; }
    public decimal? Area { get; init; }
    public List<string> Equipment { get; init; } = new();
    public bool IsActive { get; init; }
}

