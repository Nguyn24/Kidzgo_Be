using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Homework.SubmitHomework;

public sealed class SubmitHomeworkCommand : ICommand<SubmitHomeworkResponse>
{
    public Guid HomeworkStudentId { get; init; }
    public string? TextAnswer { get; init; }
    public List<string>? AttachmentUrls { get; init; }
    public string? LinkUrl { get; init; }
    public List<string>? Links { get; init; }
}

