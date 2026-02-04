using Kidzgo.Domain.Exams;

namespace Kidzgo.API.Requests;

public sealed class UpdateExerciseQuestionRequest
{
    public int? OrderIndex { get; init; }
    public string? QuestionText { get; init; }
    public QuestionType? QuestionType { get; init; }
    public string? Options { get; init; }
    public string? CorrectAnswer { get; init; }
    public int? Points { get; init; }
    public string? Explanation { get; init; }
}


