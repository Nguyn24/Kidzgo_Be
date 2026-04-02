namespace Kidzgo.API.Requests;

public sealed class AiGradeHomeworkRequest
{
    public string Language { get; init; } = "vi";
    public string? Instructions { get; init; }
    public string? Rubric { get; init; }
    public string? ExpectedAnswerText { get; init; }
}
