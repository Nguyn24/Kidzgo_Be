using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlanTemplates.UpdateLessonPlanTemplate;

public sealed class UpdateLessonPlanTemplateCommand : ICommand<UpdateLessonPlanTemplateResponse>
{
    public Guid Id { get; init; }
    public string? Level { get; init; }
    public int? SessionIndex { get; init; }
    public string? Attachment { get; init; }
    public bool? IsActive { get; init; }
}