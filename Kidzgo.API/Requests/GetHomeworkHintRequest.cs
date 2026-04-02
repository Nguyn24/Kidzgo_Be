namespace Kidzgo.API.Requests;

public sealed class GetHomeworkHintRequest
{
    public string? CurrentAnswerText { get; init; }
    public string Language { get; init; } = "vi";
}
