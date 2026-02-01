namespace Kidzgo.API.Requests;

public sealed class CreateLessonPlanTemplateRequest
{
    public Guid ProgramId { get; init; }
    public string? Level { get; init; }
    public int SessionIndex { get; init; }
    public string? StructureJson { get; init; }
}