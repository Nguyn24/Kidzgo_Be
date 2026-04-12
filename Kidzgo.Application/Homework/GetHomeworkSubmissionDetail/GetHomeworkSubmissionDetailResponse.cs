using Kidzgo.Application.Homework.Shared;
using Kidzgo.Domain.LessonPlans;

namespace Kidzgo.Application.Homework.GetHomeworkSubmissionDetail;

public sealed class GetHomeworkSubmissionDetailResponse
{
    public Guid Id { get; init; }
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
    public bool AllowResubmit { get; init; }
    public int MaxAttempts { get; init; }
    public bool AiHintEnabled { get; init; }
    public bool AiRecommendEnabled { get; init; }
    public string? SpeakingMode { get; init; }
    public List<string> TargetWords { get; init; } = new();
    public string? SpeakingExpectedText { get; init; }
    public string Status { get; init; } = null!;
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
    public int AttemptCount { get; init; }
    public List<HomeworkSubmissionAttemptDto> Attempts { get; init; } = new();

    // Student info
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public bool IsListeningQuiz => HomeworkDeliveryMetadata.IsListeningQuiz(Skills, AssignmentAttachmentUrl);
}

