namespace Kidzgo.Application.Homework.AiGradeHomework;

public sealed class AiGradeHomeworkResponse
{
    public Guid Id { get; init; }
    public Guid AssignmentId { get; init; }
    public bool IsSpeakingAnalysis { get; init; }
    public bool AiUsed { get; init; }
    public bool Persisted { get; init; }
    public string Status { get; init; } = null!;
    public decimal? Score { get; init; }
    public decimal RawAiScore { get; init; }
    public decimal RawAiMaxScore { get; init; }
    public string Summary { get; init; } = string.Empty;
    public List<string> Strengths { get; init; } = new();
    public List<string> Issues { get; init; } = new();
    public List<string> Suggestions { get; init; } = new();
    public string? ExtractedStudentAnswer { get; init; }
    public int? Stars { get; init; }
    public decimal? PronunciationScore { get; init; }
    public decimal? FluencyScore { get; init; }
    public decimal? AccuracyScore { get; init; }
    public List<string> MispronouncedWords { get; init; } = new();
    public List<SpeakingWordFeedbackDto> WordFeedback { get; init; } = new();
    public List<string> PracticePlan { get; init; } = new();
    public Dictionary<string, float> Confidence { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
    public DateTime? GradedAt { get; init; }
}

public sealed class SpeakingWordFeedbackDto
{
    public string Word { get; init; } = string.Empty;
    public string? HeardAs { get; init; }
    public string Issue { get; init; } = string.Empty;
    public string Tip { get; init; } = string.Empty;
}
