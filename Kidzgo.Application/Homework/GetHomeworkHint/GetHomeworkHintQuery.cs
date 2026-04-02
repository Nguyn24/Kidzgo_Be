using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Homework.GetHomeworkHint;

public sealed class GetHomeworkHintQuery : IQuery<GetHomeworkHintResponse>
{
    public Guid HomeworkStudentId { get; init; }
    public string? CurrentAnswerText { get; init; }
    public string Language { get; init; } = "vi";
}
