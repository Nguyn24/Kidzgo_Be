namespace Kidzgo.API.Requests;

public sealed class SaveExamSubmissionAnswerRequest
{
    public Guid QuestionId { get; set; }
    public string Answer { get; set; } = null!;
}


