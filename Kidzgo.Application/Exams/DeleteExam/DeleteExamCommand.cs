using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exams.DeleteExam;

public sealed class DeleteExamCommand : ICommand<DeleteExamResponse>
{
    public Guid Id { get; init; }
}

