namespace Kidzgo.API.Requests;

public sealed class GetHomeworkRecommendationsRequest
{
    public string? CurrentAnswerText { get; init; }
    public string Language { get; init; } = "vi";
    public int MaxItems { get; init; } = 5;
}
