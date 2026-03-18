using Kidzgo.Application.Homework.Shared;

namespace Kidzgo.Application.Homework.SubmitMultipleChoiceHomework;

public sealed class SubmitMultipleChoiceHomeworkResponse
{
    public Guid Id { get; init; }
    public Guid AssignmentId { get; init; }
    public string Status { get; init; } = null!;
    public DateTime SubmittedAt { get; init; }
    public DateTime GradedAt { get; init; }
    public decimal? MaxScore { get; init; }
    public decimal? Score { get; init; }
    public int? RewardStars { get; init; }
    public int CorrectCount { get; init; }
    public int WrongCount { get; init; }
    public int SkippedCount { get; init; }
    public int TotalCount { get; init; }
    public int TotalPoints { get; init; }
    public int EarnedPoints { get; init; }
    public List<QuizAnswerResultDto> AnswerResults { get; init; } = new();
}

