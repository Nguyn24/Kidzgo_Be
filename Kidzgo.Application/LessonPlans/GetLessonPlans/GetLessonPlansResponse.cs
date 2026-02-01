using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.LessonPlans.GetLessonPlans;

public sealed class GetLessonPlansResponse
{
    public Page<LessonPlanDto> LessonPlans { get; init; } = null!;
}

public sealed class LessonPlanDto
{
    public Guid Id { get; init; }
    public Guid SessionId { get; init; }
    public string? SessionTitle { get; init; }
    public DateTime? SessionDate { get; init; }
    public Guid? ClassId { get; init; }
    public string? ClassCode { get; init; }
    public Guid? TemplateId { get; init; }
    public string? TemplateLevel { get; init; }
    public int? TemplateSessionIndex { get; init; }
    public string? PlannedContent { get; init; }
    public string? ActualContent { get; init; }
    public string? ActualHomework { get; init; }
    public Guid? SubmittedBy { get; init; }
    public string? SubmittedByName { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}