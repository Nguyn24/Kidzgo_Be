using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exams.GetExamResultById;

public sealed class GetExamResultByIdQuery : IQuery<GetExamResultByIdResponse>
{
    public Guid Id { get; init; }
}

