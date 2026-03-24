namespace Kidzgo.Application.LessonPlans.UpdateLessonPlanActual;

public sealed class UpdateLessonPlanActualResponse
{
    public Guid Id { get; init; }
    public Guid SessionId { get; init; }
    public string? ActualContent { get; init; }
    public string? ActualHomework { get; init; }
    public string? TeacherNotes { get; init; }
    public Guid? SubmittedBy { get; init; }
    public string? SubmittedByName { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
