namespace Kidzgo.API.Requests;

public sealed class UpdateLessonPlanActualRequest
{
    public string? ActualContent { get; init; }
    public string? ActualHomework { get; init; }
    public string? TeacherNotes { get; init; }
}