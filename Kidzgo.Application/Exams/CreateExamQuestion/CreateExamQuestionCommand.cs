using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Exams;

namespace Kidzgo.Application.Exams.CreateExamQuestion;

public sealed class CreateExamQuestionCommand : ICommand<CreateExamQuestionResponse>
{
    public Guid ExamId { get; init; }
    public int OrderIndex { get; init; }
    public string QuestionText { get; init; } = null!;
    public QuestionType QuestionType { get; init; }
    public string? Options { get; init; } // JSON array for multiple choice
    public string? CorrectAnswer { get; init; }
    public int Points { get; init; }
    public string? Explanation { get; init; }
}


