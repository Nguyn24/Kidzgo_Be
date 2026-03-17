namespace Kidzgo.Application.Homework.SubmitMultipleChoiceHomework;

public sealed class SubmitMultipleChoiceHomeworkResponse
{
    public Guid Id { get; init; }
    public Guid AssignmentId { get; init; }
    public string Status { get; init; } = null!;
    public DateTime SubmittedAt { get; init; }
    public decimal? MaxScore { get; init; }
    public decimal? Score { get; init; }
    public int? RewardStars { get; init; }
    public int CorrectCount { get; init; }
    public int TotalCount { get; init; }
    public int TotalPoints { get; init; }
    public int EarnedPoints { get; init; }
    public List<AnswerResultDto> AnswerResults { get; init; } = new();
}

public class AnswerResultDto
{
    public Guid QuestionId { get; init; }
    public string QuestionText { get; init; } = null!;
    public string StudentAnswer { get; init; } = null!;
    public string CorrectAnswer { get; init; } = null!;
    public bool IsCorrect { get; init; }
    public int Points { get; init; }
    public string? Explanation { get; init; }
}

