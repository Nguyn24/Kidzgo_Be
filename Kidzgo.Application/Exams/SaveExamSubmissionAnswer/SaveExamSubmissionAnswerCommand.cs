using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exams.SaveExamSubmissionAnswer;

public sealed class SaveExamSubmissionAnswerCommand : ICommand<SaveExamSubmissionAnswerResponse>
{
    public Guid SubmissionId { get; init; }
    public Guid QuestionId { get; init; }
    public string Answer { get; init; } = null!;
}


