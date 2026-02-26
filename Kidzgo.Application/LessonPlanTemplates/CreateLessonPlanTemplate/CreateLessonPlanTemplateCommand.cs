using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlanTemplates.CreateLessonPlanTemplate;

public sealed class CreateLessonPlanTemplateCommand : ICommand<CreateLessonPlanTemplateResponse>
{
    public Guid ProgramId { get; init; }
    public string? Level { get; init; }
    public int SessionIndex { get; init; }
    public string? Attachment { get; init; }
}