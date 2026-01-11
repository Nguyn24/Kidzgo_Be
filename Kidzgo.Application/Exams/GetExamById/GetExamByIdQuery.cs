using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exams.GetExamById;

public sealed class GetExamByIdQuery : IQuery<GetExamByIdResponse>
{
    public Guid Id { get; init; }
}

