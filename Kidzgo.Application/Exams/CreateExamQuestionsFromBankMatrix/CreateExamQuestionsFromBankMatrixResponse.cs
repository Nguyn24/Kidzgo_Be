namespace Kidzgo.Application.Exams.CreateExamQuestionsFromBankMatrix;

public sealed class CreateExamQuestionsFromBankMatrixResponse
{
    public Guid ExamId { get; init; }
    public Guid ClassId { get; init; }
    public Guid ProgramId { get; init; }
    public string ExamType { get; init; } = null!;
    public string QuestionType { get; init; } = null!;
    public string? Skill { get; init; }
    public string? Topic { get; init; }
    public int RequestedQuestionCount { get; init; }
    public int CreatedQuestionCount { get; init; }
    public int TotalPoints { get; init; }
    public bool ReplacedExistingQuestions { get; init; }
    public int PreviousQuestionCount { get; init; }
    public List<ExamQuestionMatrixDistributionDto> Distribution { get; init; } = new();
    public List<GeneratedExamQuestionDto> Questions { get; init; } = new();
}

public sealed class ExamQuestionMatrixDistributionDto
{
    public string Level { get; init; } = null!;
    public int RequestedCount { get; init; }
    public int CreatedCount { get; init; }
}

public sealed class GeneratedExamQuestionDto
{
    public Guid Id { get; init; }
    public Guid SourceQuestionBankItemId { get; init; }
    public int OrderIndex { get; init; }
    public string QuestionText { get; init; } = null!;
    public string QuestionType { get; init; } = null!;
    public string Level { get; init; } = null!;
    public int Points { get; init; }
    public List<string> Options { get; init; } = new();
    public string? CorrectAnswer { get; init; }
    public string? Explanation { get; init; }
}
