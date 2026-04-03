namespace Kidzgo.Application.Homework.SubmitHomework;

public sealed class SubmitHomeworkResponse
{
    public Guid Id { get; init; }
    public Guid AssignmentId { get; init; }
    public string Status { get; init; } = null!;
    public DateTime SubmittedAt { get; init; }
    public Guid SubmissionId => Id;
    public List<string> AttachmentUrls { get; init; } = new();
    public List<string> Links { get; init; } = new();
}

