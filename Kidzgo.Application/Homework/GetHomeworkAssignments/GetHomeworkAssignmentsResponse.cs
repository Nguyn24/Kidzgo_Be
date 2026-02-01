using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Homework.GetHomeworkAssignments;

public sealed class GetHomeworkAssignmentsResponse
{
    public Page<HomeworkAssignmentDto> HomeworkAssignments { get; init; } = null!;
}

public sealed class HomeworkAssignmentDto
{
    public Guid Id { get; init; }
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public Guid? SessionId { get; init; }
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
    public DateTime CreatedAt { get; init; }
    public int TotalStudents { get; init; }
    public int SubmittedCount { get; init; }
    public int GradedCount { get; init; }
    public int LateCount { get; init; }
    public int MissingCount { get; init; }
}

