namespace Kidzgo.Application.QuestionBank;

public sealed class GenerateAiQuestionBankItemsResponse
{
    public bool AiUsed { get; init; }
    public string Summary { get; init; } = string.Empty;
    public List<GeneratedQuestionBankItemDto> Items { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
}

public sealed class GeneratedQuestionBankItemDto
{
    public string QuestionText { get; init; } = string.Empty;
    public string QuestionType { get; init; } = "MultipleChoice";
    public List<string> Options { get; init; } = new();
    public string CorrectAnswer { get; init; } = string.Empty;
    public int Points { get; init; }
    public string? Explanation { get; init; }
    public string? Topic { get; init; }
    public string? Skill { get; init; }
    public List<string> GrammarTags { get; init; } = new();
    public List<string> VocabularyTags { get; init; } = new();
    public string Level { get; init; } = "Medium";
}
