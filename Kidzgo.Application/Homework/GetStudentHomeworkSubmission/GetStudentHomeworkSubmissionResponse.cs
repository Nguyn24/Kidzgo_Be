using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.Homework.Shared;

namespace Kidzgo.Application.Homework.GetStudentHomeworkSubmission;

public sealed class GetStudentHomeworkSubmissionResponse
{
    public Guid Id { get; init; }
    public Guid HomeworkStudentId { get; init; }
    public Guid AssignmentId { get; init; }
    public string AssignmentTitle { get; init; } = null!;
    public string? AssignmentDescription { get; init; }
    public string? AssignmentAttachmentUrl { get; init; }
    public string? Instructions { get; init; }
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public DateTime? DueAt { get; init; }
    public string? Book { get; init; }
    public string? Pages { get; init; }
    public string? Skills { get; init; }
    public string SubmissionType { get; init; } = null!;
    public decimal? MaxScore { get; init; }
    public int? RewardStars { get; init; }
    public int? TimeLimitMinutes { get; init; }
    public bool AllowResubmit { get; init; }
    public string Status { get; init; } = null!;
    public DateTime? StartedAt { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime? GradedAt { get; init; }
    public decimal? Score { get; init; }
    public string? TeacherFeedback { get; init; }
    public string? AiFeedback { get; init; }
    public string? AttachmentUrls { get; init; }
    public string? TextAnswer { get; init; }
    public string? LinkUrl { get; init; }
    public bool IsLate { get; init; }
    public bool IsOverdue { get; init; }
    public List<HomeworkQuestionDto> Questions { get; init; } = new();
    public HomeworkReviewDto? Review { get; init; }
    public bool ShowReview { get; init; }
    public bool ShowCorrectAnswer { get; init; }
    public bool ShowExplanation { get; init; }
}

