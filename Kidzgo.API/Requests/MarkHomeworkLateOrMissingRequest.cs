namespace Kidzgo.API.Requests;

public sealed class MarkHomeworkLateOrMissingRequest
{
    public Guid HomeworkStudentId { get; init; }
    public string Status { get; init; } = null!; // "LATE" or "MISSING"
}

