namespace Kidzgo.API.Requests;

public sealed class GetHomeworkSpeakingAnalysisRequest
{
    public string? CurrentTranscript { get; init; }
    public string Language { get; init; } = "vi";
}
