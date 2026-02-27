namespace Kidzgo.Application.LessonPlanTemplates.GetLessonPlanTemplateById;

public sealed class GetLessonPlanTemplateByIdResponse
{
    public Guid Id { get; init; }
    public Guid ProgramId { get; init; }
    public string? ProgramName { get; init; }
    public string? Level { get; init; }
    public int SessionIndex { get; init; }
    public string? Attachment { get; init; }
    public bool IsActive { get; init; }
    public Guid? CreatedBy { get; init; }
    public string? CreatedByName { get; init; }
    public DateTime CreatedAt { get; init; }
    public int UsedCount { get; init; }
}