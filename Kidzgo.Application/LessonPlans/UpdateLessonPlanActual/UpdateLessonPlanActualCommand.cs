using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlans.UpdateLessonPlanActual;

public sealed class UpdateLessonPlanActualCommand : ICommand<UpdateLessonPlanActualResponse>
{
    public Guid Id { get; init; }
    public string? ActualContent { get; init; }
    public string? ActualHomework { get; init; }
    public string? TeacherNotes { get; init; }
}