namespace Kidzgo.API.Requests;

public sealed class GenerateQuestionBankItemsRequest
{
    public Guid ProgramId { get; init; }
    public string Topic { get; init; } = string.Empty;
    public string QuestionType { get; init; } = "MultipleChoice";
    public int QuestionCount { get; init; } = 5;
    public string Level { get; init; } = "Medium";
    public string? Skill { get; init; }
    public string TaskStyle { get; init; } = "standard";
    public List<string> GrammarTags { get; init; } = new();
    public List<string> VocabularyTags { get; init; } = new();
    public string? Instructions { get; init; }
    public string Language { get; init; } = "vi";
    public int PointsPerQuestion { get; init; } = 1;
}
