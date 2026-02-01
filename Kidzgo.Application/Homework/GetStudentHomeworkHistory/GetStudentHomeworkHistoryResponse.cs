using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Homework.GetStudentHomeworkHistory;

public sealed class GetStudentHomeworkHistoryResponse
{
    public Page<StudentHomeworkHistoryDto> Homeworks { get; init; } = null!;
}

public sealed class StudentHomeworkHistoryDto
{
    public Guid Id { get; init; }
    public Guid AssignmentId { get; init; }
    public string AssignmentTitle { get; init; } = null!;
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public DateTime? DueAt { get; init; }
    public string Status { get; init; } = null!;
    public DateTime? SubmittedAt { get; init; }
    public DateTime? GradedAt { get; init; }
    public decimal? Score { get; init; }
    public decimal? MaxScore { get; init; }
    public string? TeacherFeedback { get; init; }
    public bool IsLate { get; init; }
}

