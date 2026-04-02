namespace Kidzgo.Application.Homework.GetHomeworkRecommendations;

public sealed class GetHomeworkRecommendationsResponse
{
    public bool AiUsed { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string FocusSkill { get; init; } = string.Empty;
    public List<string> Topics { get; init; } = new();
    public List<string> GrammarTags { get; init; } = new();
    public List<string> VocabularyTags { get; init; } = new();
    public List<string> RecommendedLevels { get; init; } = new();
    public List<string> PracticeTypes { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
    public List<RecommendedPracticeItemDto> Items { get; init; } = new();
}

public sealed class RecommendedPracticeItemDto
{
    public Guid QuestionBankItemId { get; init; }
    public string QuestionText { get; init; } = string.Empty;
    public string QuestionType { get; init; } = string.Empty;
    public List<string> Options { get; init; } = new();
    public string? Topic { get; init; }
    public string? Skill { get; init; }
    public List<string> GrammarTags { get; init; } = new();
    public List<string> VocabularyTags { get; init; } = new();
    public string Level { get; init; } = string.Empty;
    public int Points { get; init; }
    public string Reason { get; init; } = string.Empty;
}
