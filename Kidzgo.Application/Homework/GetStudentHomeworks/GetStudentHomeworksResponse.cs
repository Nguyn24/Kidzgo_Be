using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Homework.GetStudentHomeworks;

public sealed class GetStudentHomeworksResponse
{
    public Page<StudentHomeworkDto> Homeworks { get; init; } = null!;
}

public sealed class StudentHomeworkDto
{
    public Guid Id { get; init; }
    public Guid AssignmentId { get; init; }
    public string AssignmentTitle { get; init; } = null!;
    public string? AssignmentDescription { get; init; }
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public DateTime? DueAt { get; init; }
    public string? Book { get; init; }
    public string? Pages { get; init; }
    public string? Skills { get; init; }
    public string SubmissionType { get; init; } = null!;
    public decimal? MaxScore { get; init; }
    public string Status { get; init; } = null!;
    public DateTime? SubmittedAt { get; init; }
    public DateTime? GradedAt { get; init; }
    public decimal? Score { get; init; }
    public bool IsLate { get; init; }
    public bool IsOverdue { get; init; }
}

