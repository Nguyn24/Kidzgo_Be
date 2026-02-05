using Kidzgo.Domain.Exams;

namespace Kidzgo.API.Requests;

public sealed class CreateExerciseQuestionRequest
{
    public int OrderIndex { get; init; }
    public string QuestionText { get; init; } = null!;
    public QuestionType QuestionType { get; init; }
    public string? Options { get; init; }
    public string? CorrectAnswer { get; init; }
    public int Points { get; init; }
    public string? Explanation { get; init; }
}


