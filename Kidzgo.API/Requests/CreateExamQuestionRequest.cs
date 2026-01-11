using Kidzgo.Domain.Exams;

namespace Kidzgo.API.Requests;

public sealed class CreateExamQuestionRequest
{
    public int OrderIndex { get; set; }
    public string QuestionText { get; set; } = null!;
    public QuestionType QuestionType { get; set; }
    public string? Options { get; set; } // JSON array for multiple choice
    public string? CorrectAnswer { get; set; }
    public int Points { get; set; }
    public string? Explanation { get; set; }
}


