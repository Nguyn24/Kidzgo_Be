using Microsoft.AspNetCore.Http;

namespace Kidzgo.API.Requests;

public sealed class AnalyzeSpeakingPracticeRequest
{
    public Guid? HomeworkStudentId { get; init; }
    public string Language { get; init; } = "vi";
    public string? Mode { get; init; }
    public string? Topic { get; init; }
    public string? ExpectedText { get; init; }
    public string? TargetWords { get; init; }
    public string? ConversationHistory { get; init; }
    public string? Instructions { get; init; }
    public IFormFile? File { get; init; }
}
