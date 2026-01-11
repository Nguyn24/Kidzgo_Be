namespace Kidzgo.Application.Exams.SaveExamSubmissionAnswer;

public sealed class SaveExamSubmissionAnswerResponse
{
    public Guid Id { get; init; }
    public Guid SubmissionId { get; init; }
    public Guid QuestionId { get; init; }
    public string Answer { get; init; } = null!;
    public DateTime? AnsweredAt { get; init; }
}


