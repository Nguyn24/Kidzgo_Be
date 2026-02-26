using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Homework.SubmitMultipleChoiceHomework;

public sealed class SubmitMultipleChoiceHomeworkCommand : ICommand<SubmitMultipleChoiceHomeworkResponse>
{
    public Guid HomeworkStudentId { get; init; }
    public List<SubmitAnswerDto> Answers { get; init; } = new();
}

public class SubmitAnswerDto
{
    public Guid QuestionId { get; init; }
    public string Answer { get; init; } = null!; // Option index (0, 1, 2, 3) for multiple choice, text for text input
}

