namespace Kidzgo.Application.LessonPlans.UpdateLessonPlanActual;

public sealed class UpdateLessonPlanActualResponse
{
    public Guid Id { get; init; }
    public Guid SessionId { get; init; }
    public string? ActualContent { get; init; }
    public string? ActualHomework { get; init; }
    public string? TeacherNotes { get; init; }
    public DateTime? UpdatedAt { get; init; }
}