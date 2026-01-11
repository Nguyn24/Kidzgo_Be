namespace Kidzgo.Application.Exams.CreateExamResult;

public sealed class CreateExamResultResponse
{
    public Guid Id { get; init; }
    public Guid ExamId { get; init; }
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public decimal? Score { get; init; }
    public string? Comment { get; init; }
    public List<string>? AttachmentUrls { get; init; }
    public Guid? GradedBy { get; init; }
    public string? GradedByName { get; init; }
    public DateTime? GradedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

