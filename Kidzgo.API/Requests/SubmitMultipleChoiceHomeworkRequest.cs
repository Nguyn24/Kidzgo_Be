namespace Kidzgo.API.Requests;

public sealed class SubmitMultipleChoiceHomeworkRequest
{
    public Guid HomeworkStudentId { get; init; }
    public List<SubmitAnswerRequest> Answers { get; init; } = new();
}

public class SubmitAnswerRequest
{
    public Guid QuestionId { get; init; }
    public string Answer { get; init; } = null!;
}

