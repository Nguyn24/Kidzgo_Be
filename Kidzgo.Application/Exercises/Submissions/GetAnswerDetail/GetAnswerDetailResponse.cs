namespace Kidzgo.Application.Exercises.Submissions.GetAnswerDetail;

public sealed class GetAnswerDetailResponse
{
    public Guid SubmissionId { get; init; }
    public Guid ExerciseId { get; init; }
    public Guid StudentProfileId { get; init; }

    public Guid QuestionId { get; init; }
    public int OrderIndex { get; init; }
    public string QuestionText { get; init; } = null!;
    public string QuestionType { get; init; } = null!;
    public string? Options { get; init; }
    public string? CorrectAnswer { get; init; }
    public int Points { get; init; }

    public string Answer { get; init; } = null!;
    public bool IsCorrect { get; init; }
    public decimal? PointsAwarded { get; init; }
    public string? TeacherFeedback { get; init; }
}


