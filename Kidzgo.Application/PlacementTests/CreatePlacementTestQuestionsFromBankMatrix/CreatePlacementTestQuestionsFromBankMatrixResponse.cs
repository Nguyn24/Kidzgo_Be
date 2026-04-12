namespace Kidzgo.Application.PlacementTests.CreatePlacementTestQuestionsFromBankMatrix;

public sealed class CreatePlacementTestQuestionsFromBankMatrixResponse
{
    public Guid PlacementTestId { get; init; }
    public Guid ProgramId { get; init; }
    public string ProgramSource { get; init; } = null!;
    public string QuestionType { get; init; } = null!;
    public string? Skill { get; init; }
    public string? Topic { get; init; }
    public int RequestedQuestionCount { get; init; }
    public int CreatedQuestionCount { get; init; }
    public int TotalPoints { get; init; }
    public List<PlacementTestQuestionMatrixDistributionDto> Distribution { get; init; } = new();
    public List<GeneratedPlacementTestQuestionDto> Questions { get; init; } = new();
}

public sealed class PlacementTestQuestionMatrixDistributionDto
{
    public string Level { get; init; } = null!;
    public int RequestedCount { get; init; }
    public int CreatedCount { get; init; }
}

public sealed class GeneratedPlacementTestQuestionDto
{
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
