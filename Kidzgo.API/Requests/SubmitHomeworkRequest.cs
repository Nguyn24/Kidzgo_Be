namespace Kidzgo.API.Requests;

public sealed class SubmitHomeworkRequest
{
    public Guid HomeworkStudentId { get; init; }
    public string? TextAnswer { get; init; }
    public List<string>? AttachmentUrls { get; init; }
    public string? LinkUrl { get; init; }
    public List<string>? Links { get; init; }
}

