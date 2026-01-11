using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exams.DeleteExamQuestion;

public sealed class DeleteExamQuestionCommand : ICommand<DeleteExamQuestionResponse>
{
    public Guid QuestionId { get; init; }
}


