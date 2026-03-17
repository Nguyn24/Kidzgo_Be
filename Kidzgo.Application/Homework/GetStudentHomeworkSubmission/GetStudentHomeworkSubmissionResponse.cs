using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Homework.GetStudentHomeworkSubmission;

public sealed class GetStudentHomeworkSubmissionResponse
{
    public Guid Id { get; init; }
    public Guid AssignmentId { get; init; }
    public string AssignmentTitle { get; init; } = null!;
    public string? AssignmentDescription { get; init; }
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
    public List<StudentHomeworkQuestionDto> Questions { get; init; } = new();
}

public sealed class StudentHomeworkQuestionDto
{
    public Guid Id { get; init; }
    public int OrderIndex { get; init; }
    public string QuestionText { get; init; } = null!;
    public string QuestionType { get; init; } = null!;
    public List<string> Options { get; init; } = new();
    public int Points { get; init; }
}

