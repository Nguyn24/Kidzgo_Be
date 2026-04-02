namespace Kidzgo.Application.Homework.GetHomeworkHint;

public sealed class GetHomeworkHintResponse
{
    public bool AiUsed { get; init; }
    public string Summary { get; init; } = string.Empty;
    public List<string> Hints { get; init; } = new();
    public List<string> GrammarFocus { get; init; } = new();
    public List<string> VocabularyFocus { get; init; } = new();
    public string Encouragement { get; init; } = string.Empty;
    public List<string> Warnings { get; init; } = new();
}
