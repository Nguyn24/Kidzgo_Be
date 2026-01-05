using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Classrooms.GetClassrooms;

public sealed class GetClassroomsResponse
{
    public Page<ClassroomDto> Classrooms { get; init; } = null!;
}

public sealed class ClassroomDto
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public string Name { get; init; } = null!;
    public int Capacity { get; init; }
    public string? Note { get; init; }
    public bool IsActive { get; init; }
}

