using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Homework.GetHomeworkSpeakingAnalysis;

public sealed class GetHomeworkSpeakingAnalysisQuery : IQuery<GetHomeworkSpeakingAnalysisResponse>
{
    public Guid HomeworkStudentId { get; init; }
    public string? CurrentTranscript { get; init; }
    public string Language { get; init; } = "vi";
}
