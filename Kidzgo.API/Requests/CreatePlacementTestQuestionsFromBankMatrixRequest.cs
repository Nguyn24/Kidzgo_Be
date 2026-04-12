namespace Kidzgo.API.Requests;

public sealed class CreatePlacementTestQuestionsFromBankMatrixRequest
{
    public Guid? ProgramId { get; init; }
    public string? QuestionType { get; init; } = "MultipleChoice";
    public string? Skill { get; init; }
    public string? Topic { get; init; }
    public bool ShuffleQuestions { get; init; } = true;
    public int TotalQuestions { get; init; }
    public List<PlacementTestQuestionLevelDistributionRequest> Distribution { get; init; } = new();
}

public sealed class PlacementTestQuestionLevelDistributionRequest
{
    public string Level { get; init; } = null!;
    public int Count { get; init; }
}
