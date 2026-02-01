namespace Kidzgo.Application.LessonPlans.GetLessonPlanTemplate;

public sealed class GetLessonPlanTemplateResponse
{
    public Guid LessonPlanId { get; init; }
    public Guid? TemplateId { get; init; }
    public string? TemplateLevel { get; init; }
    public int? TemplateSessionIndex { get; init; }
    public string? TemplateStructureJson { get; init; }
    public string? PlannedContent { get; init; }
    public bool IsReadOnly { get; init; } = true;
}