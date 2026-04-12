namespace Kidzgo.API.Requests;

public sealed class CreateExamQuestionsFromBankMatrixRequest
{
    public int TotalQuestions { get; init; }
    public string? QuestionType { get; init; } = "MultipleChoice";
    public string? Skill { get; init; }
    public string? Topic { get; init; }
    public bool ReplaceExistingQuestions { get; init; } = true;
    public bool ShuffleQuestions { get; init; } = true;
    public List<ExamQuestionLevelDistributionRequest> Distribution { get; init; } = new();
}

public sealed class ExamQuestionLevelDistributionRequest
{
    public string Level { get; init; } = null!;
    public int Count { get; init; }
}
