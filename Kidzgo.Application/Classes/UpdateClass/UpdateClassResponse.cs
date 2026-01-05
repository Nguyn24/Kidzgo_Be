using Kidzgo.Domain.Classes;

namespace Kidzgo.Application.Classes.UpdateClass;

public sealed class UpdateClassResponse
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public Guid ProgramId { get; init; }
    public string Code { get; init; } = null!;
    public string Title { get; init; } = null!;
    public Guid? MainTeacherId { get; init; }
    public Guid? AssistantTeacherId { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public ClassStatus Status { get; init; }
    public int Capacity { get; init; }
    public string? SchedulePattern { get; init; }
}

