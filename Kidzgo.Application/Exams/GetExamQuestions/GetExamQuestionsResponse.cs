using Kidzgo.Domain.Exams;

namespace Kidzgo.Application.Exams.GetExamQuestions;

public sealed class GetExamQuestionsResponse
{
    public List<ExamQuestionDto> Questions { get; init; } = new();
}

public sealed class ExamQuestionDto
{
    public Guid Id { get; init; }
    public Guid ExamId { get; init; }
    public int OrderIndex { get; init; }
    public string QuestionText { get; init; } = null!;
    public QuestionType QuestionType { get; init; }
    public string? Options { get; init; }
    public string? CorrectAnswer { get; init; }
    public int Points { get; init; }
    public string? Explanation { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}


