using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Homework.GetHomeworkSpeakingAnalysis;

namespace Kidzgo.Application.Homework.AnalyzeSpeakingPractice;

public sealed class AnalyzeSpeakingPracticeQuery : IQuery<GetHomeworkSpeakingAnalysisResponse>
{
    public Guid? HomeworkStudentId { get; init; }
    public byte[] FileBytes { get; init; } = Array.Empty<byte>();
    public string FileName { get; init; } = "speaking-practice";
    public string ContentType { get; init; } = "application/octet-stream";
    public string Language { get; init; } = "vi";
    public string? Mode { get; init; }
    public string? ExpectedText { get; init; }
    public string? TargetWords { get; init; }
    public string? Instructions { get; init; }
}
