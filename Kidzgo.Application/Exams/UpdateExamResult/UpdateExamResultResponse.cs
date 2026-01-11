namespace Kidzgo.Application.Exams.UpdateExamResult;

public sealed class UpdateExamResultResponse
{
    public Guid Id { get; init; }
    public Guid ExamId { get; init; }
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public decimal? Score { get; init; }
    public string? Comment { get; init; }
    public List<string>? AttachmentUrls { get; init; }
    public DateTime UpdatedAt { get; init; }
}

