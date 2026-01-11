using Kidzgo.Domain.Exams;

namespace Kidzgo.API.Requests;

public sealed class UpdateExamQuestionRequest
{
    public int? OrderIndex { get; set; }
    public string? QuestionText { get; set; }
    public QuestionType? QuestionType { get; set; }
    public string? Options { get; set; }
    public string? CorrectAnswer { get; set; }
    public int? Points { get; set; }
    public string? Explanation { get; set; }
}


