namespace Kidzgo.API.Requests;

public sealed class UpdateLessonPlanTemplateRequest
{
    public string? Level { get; init; }
    public int? SessionIndex { get; init; }
    public string? Attachment { get; init; }
    public bool? IsActive { get; init; }
}