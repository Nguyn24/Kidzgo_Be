namespace Kidzgo.Application.Homework.Shared;

public sealed class HomeworkSubmissionAttemptDto
{
    public Guid Id { get; init; }
    public int AttemptNumber { get; init; }
    public string Status { get; init; } = null!;
    public DateTime? StartedAt { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime? GradedAt { get; init; }
    public decimal? Score { get; init; }
    public string? TeacherFeedback { get; init; }
    public string? AiFeedback { get; init; }
    public string? TextAnswer { get; init; }
    public string? AttachmentUrl { get; init; }
    public bool IsLatest { get; init; }
}
