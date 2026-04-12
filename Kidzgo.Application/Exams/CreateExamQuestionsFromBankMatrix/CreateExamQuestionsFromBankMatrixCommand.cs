using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Homework;

namespace Kidzgo.Application.Exams.CreateExamQuestionsFromBankMatrix;

public sealed class CreateExamQuestionsFromBankMatrixCommand : ICommand<CreateExamQuestionsFromBankMatrixResponse>
{
    public Guid ExamId { get; init; }
    public int TotalQuestions { get; init; }
    public QuestionType QuestionType { get; init; } = QuestionType.MultipleChoice;
    public string? Skill { get; init; }
    public string? Topic { get; init; }
    public bool ReplaceExistingQuestions { get; init; } = true;
    public bool ShuffleQuestions { get; init; } = true;
    public List<ExamQuestionMatrixLevelCountDto> Distribution { get; init; } = new();
}

public sealed class ExamQuestionMatrixLevelCountDto
{
    public QuestionLevel Level { get; init; }
    public int Count { get; init; }
}
