namespace Kidzgo.API.Requests;

public sealed class SaveExerciseAnswerRequest
{
    public Guid SubmissionId { get; init; }
    public Guid QuestionId { get; init; }
    public string Answer { get; init; } = null!;
}


