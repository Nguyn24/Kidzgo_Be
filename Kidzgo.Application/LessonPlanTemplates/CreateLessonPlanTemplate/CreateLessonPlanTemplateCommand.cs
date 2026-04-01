using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlanTemplates.CreateLessonPlanTemplate;

public sealed class CreateLessonPlanTemplateCommand : ICommand<CreateLessonPlanTemplateResponse>
{
    public Guid ProgramId { get; init; }
    public string? Level { get; init; }
    public string? Title { get; init; }
    public int SessionIndex { get; init; }
    public string? SyllabusMetadata { get; init; }
    public string? SyllabusContent { get; init; }
    public string? SourceFileName { get; init; }
    public string? Attachment { get; init; }
}
