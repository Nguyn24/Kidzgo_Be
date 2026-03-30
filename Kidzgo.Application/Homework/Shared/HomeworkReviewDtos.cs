namespace Kidzgo.Application.Homework.Shared;

public sealed class HomeworkQuestionDto
{
    public Guid Id { get; init; }
    public int OrderIndex { get; init; }
    public string QuestionText { get; init; } = null!;
    public string QuestionType { get; init; } = null!;
    public List<HomeworkOptionDto> Options { get; init; } = new();
    public int Points { get; init; }
}

public sealed class HomeworkOptionDto
{
    public Guid Id { get; init; }
    public string Text { get; init; } = null!;
    public int OrderIndex { get; init; }
}

public sealed class HomeworkReviewDto
{
    public List<QuizAnswerResultDto> AnswerResults { get; init; } = new();
}
