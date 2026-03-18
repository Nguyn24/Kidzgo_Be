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
    public Guid? SelectedOptionId { get; init; }
}

