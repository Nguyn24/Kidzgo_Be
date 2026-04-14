using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Homework;

namespace Kidzgo.Application.QuestionBank;

public sealed class GenerateAiQuestionBankItemsQuery : IQuery<GenerateAiQuestionBankItemsResponse>
{
    public Guid ProgramId { get; init; }
    public string Topic { get; init; } = string.Empty;
    public HomeworkQuestionType QuestionType { get; init; } = HomeworkQuestionType.MultipleChoice;
    public int QuestionCount { get; init; } = 5;
    public QuestionLevel Level { get; init; } = QuestionLevel.Medium;
    public string? Skill { get; init; }
    public string TaskStyle { get; init; } = "standard";
    public List<string> GrammarTags { get; init; } = new();
    public List<string> VocabularyTags { get; init; } = new();
    public string? Instructions { get; init; }
    public string Language { get; init; } = "vi";
    public int PointsPerQuestion { get; init; } = 1;
    public string? SourceText { get; init; }
    public string? SourceFileName { get; init; }
    public Stream? SourceFileStream { get; init; }
}
