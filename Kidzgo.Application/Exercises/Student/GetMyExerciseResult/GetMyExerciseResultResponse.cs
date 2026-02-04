namespace Kidzgo.Application.Exercises.Student.GetMyExerciseResult;

public sealed class GetMyExerciseResultResponse
{
    public Guid SubmissionId { get; init; }
    public Guid ExerciseId { get; init; }
    public string ExerciseTitle { get; init; } = null!;
    public decimal? Score { get; init; }
    public DateTime SubmittedAt { get; init; }
    public DateTime? GradedAt { get; init; }
    public IReadOnlyList<MyExerciseAnswerResultItem> Answers { get; init; } = Array.Empty<MyExerciseAnswerResultItem>();
}

public sealed class MyExerciseAnswerResultItem
{
    public Guid QuestionId { get; init; }
    public int OrderIndex { get; init; }
    public string QuestionText { get; init; } = null!;
    public string QuestionType { get; init; } = null!;
    public int Points { get; init; }
    public string Answer { get; init; } = null!;
    public bool IsCorrect { get; init; }
    public decimal? PointsAwarded { get; init; }
    public string? TeacherFeedback { get; init; }
}


