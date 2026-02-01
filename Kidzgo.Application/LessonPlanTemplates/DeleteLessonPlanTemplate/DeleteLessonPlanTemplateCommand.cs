using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlanTemplates.DeleteLessonPlanTemplate;

public sealed class DeleteLessonPlanTemplateCommand : ICommand
{
    public Guid Id { get; init; }
}