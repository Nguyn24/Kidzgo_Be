namespace Kidzgo.Application.Homework.GetHomeworkAssignmentById;

public sealed class GetHomeworkAssignmentByIdResponse
{
    public Guid Id { get; init; }
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public Guid? SessionId { get; init; }
    public string? SessionTitle { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public DateTime? DueAt { get; init; }
    public string? Book { get; init; }
    public string? Pages { get; init; }
    public string? Skills { get; init; }
    public string SubmissionType { get; init; } = null!;
    public decimal? MaxScore { get; init; }
    public int? RewardStars { get; init; }
    public Guid? MissionId { get; init; }
    public string? MissionTitle { get; init; }
    public string? Instructions { get; init; }
    public string? ExpectedAnswer { get; init; }
    public string? Rubric { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<HomeworkStudentDto> Students { get; init; } = new();
}

public sealed class HomeworkStudentDto
{
    public Guid Id { get; init; }
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public string Status { get; init; } = null!;
    public DateTime? SubmittedAt { get; init; }
    public DateTime? GradedAt { get; init; }
    public decimal? Score { get; init; }
    public string? TeacherFeedback { get; init; }
}

