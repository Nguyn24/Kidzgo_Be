using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Homework;

public class HomeworkSubmissionAttempt : Entity
{
    public Guid Id { get; set; }
    public Guid HomeworkStudentId { get; set; }
    public int AttemptNumber { get; set; }
    public HomeworkStatus Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? GradedAt { get; set; }
    public decimal? Score { get; set; }
    public string? TeacherFeedback { get; set; }
    public string? AiFeedback { get; set; }
    public string? TextAnswer { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    public HomeworkStudent HomeworkStudent { get; set; } = null!;
}
