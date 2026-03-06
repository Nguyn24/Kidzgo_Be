namespace Kidzgo.Application.LessonPlanTemplates.CreateLessonPlanTemplate;

public sealed class CreateLessonPlanTemplateResponse
{
    public Guid Id { get; init; }
    public Guid ProgramId { get; init; }
    public string? Level { get; init; }
    public string? Title { get; init; }
    public int SessionIndex { get; init; }
    public string? Attachment { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}