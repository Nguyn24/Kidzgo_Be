namespace Kidzgo.Application.Exams.GetStudentExamResults;

public sealed class StudentExamResultDto
{
    public Guid Id { get; init; }
    public Guid ExamId { get; init; }
    public string ExamType { get; init; } = null!;
    public DateOnly ExamDate { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public decimal? Score { get; init; }
    public decimal? MaxScore { get; init; }
    public string? Comment { get; init; }
    public List<string>? AttachmentUrls { get; init; }
    public DateTime? GradedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

