namespace Kidzgo.Application.Homework.GetHomeworkSpeakingAnalysis;

public sealed class GetHomeworkSpeakingAnalysisResponse
{
    public bool AiUsed { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string Transcript { get; init; } = string.Empty;
    public decimal OverallScore { get; init; }
    public decimal PronunciationScore { get; init; }
    public decimal FluencyScore { get; init; }
    public decimal AccuracyScore { get; init; }
    public int Stars { get; init; }
    public List<string> Strengths { get; init; } = new();
    public List<string> Issues { get; init; } = new();
    public List<string> MispronouncedWords { get; init; } = new();
    public List<HomeworkSpeakingWordFeedbackResponse> WordFeedback { get; init; } = new();
    public List<string> Suggestions { get; init; } = new();
    public List<string> PracticePlan { get; init; } = new();
    public Dictionary<string, float> Confidence { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
}

public sealed class HomeworkSpeakingWordFeedbackResponse
{
    public string Word { get; init; } = string.Empty;
    public string? HeardAs { get; init; }
    public string Issue { get; init; } = string.Empty;
    public string Tip { get; init; } = string.Empty;
}
