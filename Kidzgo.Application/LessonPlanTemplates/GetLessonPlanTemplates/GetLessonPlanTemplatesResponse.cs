using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.LessonPlanTemplates.GetLessonPlanTemplates;

public sealed class GetLessonPlanTemplatesResponse
{
    public Page<LessonPlanTemplateDto> Templates { get; init; } = null!;
}

public sealed class LessonPlanTemplateDto
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