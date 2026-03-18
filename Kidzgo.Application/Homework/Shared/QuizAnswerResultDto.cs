namespace Kidzgo.Application.Homework.Shared;

public sealed class QuizAnswerResultDto
{
    public Guid QuestionId { get; init; }
    public string QuestionText { get; init; } = null!;
    public Guid? SelectedOptionId { get; init; }
    public string? SelectedOptionText { get; init; }
    public Guid? CorrectOptionId { get; init; }
    public string? CorrectOptionText { get; init; }
    public bool IsCorrect { get; init; }
    public int EarnedPoints { get; init; }
    public int MaxPoints { get; init; }
    public string? Explanation { get; init; }
}
