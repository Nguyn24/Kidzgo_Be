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
    public string? Topic { get; init; }
    public List<string> GrammarTags { get; init; } = new();
    public List<string> VocabularyTags { get; init; } = new();
    public string SubmissionType { get; init; } = null!;
    public decimal? MaxScore { get; init; }
    public int? RewardStars { get; init; }
    public int? TimeLimitMinutes { get; init; }
    public bool AllowResubmit { get; init; }
    public int MaxAttempts { get; init; }
    public bool AiHintEnabled { get; init; }
    public bool AiRecommendEnabled { get; init; }
    public string? SpeakingMode { get; init; }
    public List<string> TargetWords { get; init; } = new();
    public string? SpeakingExpectedText { get; init; }
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
    public Guid? AttemptId { get; init; }
    public int? AttemptNumber { get; init; }
    public int AttemptCount { get; init; }
    public List<HomeworkSubmissionAttemptDto> Attempts { get; init; } = new();
    public Guid HomeworkId => AssignmentId;
    public string Title => AssignmentTitle;
    public string? Description => AssignmentDescription;
    public DateTime? DueDate => DueAt;
    public string? Subject => Topic ?? Skills;
    public string? TeacherName { get; init; }
    public List<string> AssignmentAttachmentUrls { get; init; } = new();
    public HomeworkSubmissionPayloadDto Submission { get; init; } = new();
}

public sealed class HomeworkSubmissionPayloadDto
{
    public string? TextAnswer { get; init; }
    public List<string> AttachmentUrls { get; init; } = new();
    public List<string> Links { get; init; } = new();
    public DateTime? SubmittedAt { get; init; }
    public DateTime? GradedAt { get; init; }
    public decimal? Score { get; init; }
    public string? TeacherFeedback { get; init; }
}

