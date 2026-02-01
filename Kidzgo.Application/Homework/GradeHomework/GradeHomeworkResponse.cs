namespace Kidzgo.Application.Homework.GradeHomework;

public sealed class GradeHomeworkResponse
{
    public Guid Id { get; init; }
    public Guid AssignmentId { get; init; }
    public string Status { get; init; } = null!;
    public decimal Score { get; init; }
    public string? TeacherFeedback { get; init; }
    public DateTime GradedAt { get; init; }
}

