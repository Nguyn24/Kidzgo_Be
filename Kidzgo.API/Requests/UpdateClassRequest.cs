namespace Kidzgo.API.Requests;

public sealed class UpdateClassRequest
{
    public Guid BranchId { get; set; }
    public Guid ProgramId { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public Guid? MainTeacherId { get; set; }
    public Guid? AssistantTeacherId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int Capacity { get; set; }
    public string? SchedulePattern { get; set; }
}

