using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Homework.GetHomeworkSubmissions;

public sealed class GetHomeworkSubmissionsResponse
{
    public Page<HomeworkSubmissionDto> Submissions { get; set; } = null!;
}

public sealed class HomeworkSubmissionDto
{
    public Guid Id { get; init; }
    public Guid HomeworkAssignmentId { get; init; }
    public string HomeworkTitle { get; init; } = null!;
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public string Status { get; init; } = null!;
    public DateTime? SubmittedAt { get; init; }
    public DateTime? GradedAt { get; init; }
    public decimal? Score { get; init; }
    public string? TeacherFeedback { get; init; }
    public DateTime DueAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
