namespace Kidzgo.API.Requests;

public sealed class UpdateLessonPlanTemplateRequest
{
    public string? Level { get; init; }
    public int? SessionIndex { get; init; }
    public string? StructureJson { get; init; }
    public bool? IsActive { get; init; }
}