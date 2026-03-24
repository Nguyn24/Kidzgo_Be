using Kidzgo.Domain.Homework;

namespace Kidzgo.Application.QuestionBank;

public sealed class QuestionBankItemDto
{
    public Guid Id { get; init; }
    public Guid ProgramId { get; init; }
    public string QuestionText { get; init; } = null!;
    public string QuestionType { get; init; } = null!;
    public List<string> Options { get; init; } = new();
    public string? CorrectAnswer { get; init; }
    public int Points { get; init; }
    public string? Explanation { get; init; }
    public QuestionLevel Level { get; init; }
    public DateTime CreatedAt { get; init; }
}
