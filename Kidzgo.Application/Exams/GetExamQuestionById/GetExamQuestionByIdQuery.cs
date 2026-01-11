using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exams.GetExamQuestionById;

public sealed class GetExamQuestionByIdQuery : IQuery<GetExamQuestionByIdResponse>
{
    public Guid QuestionId { get; init; }
}


