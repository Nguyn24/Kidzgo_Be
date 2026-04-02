using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Homework.AiGradeHomework;

public sealed class AiGradeHomeworkCommand : ICommand<AiGradeHomeworkResponse>
{
    public Guid HomeworkStudentId { get; init; }
    public string Language { get; init; } = "vi";
    public string? Instructions { get; init; }
    public string? Rubric { get; init; }
    public string? ExpectedAnswerText { get; init; }
}
