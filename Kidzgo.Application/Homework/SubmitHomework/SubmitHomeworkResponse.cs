namespace Kidzgo.Application.Homework.SubmitHomework;

public sealed class SubmitHomeworkResponse
{
    public Guid Id { get; init; }
    public Guid AssignmentId { get; init; }
    public string Status { get; init; } = null!;
    public DateTime SubmittedAt { get; init; }
}

