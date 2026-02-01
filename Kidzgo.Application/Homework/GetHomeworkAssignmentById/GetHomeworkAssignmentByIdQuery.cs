using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Homework.GetHomeworkAssignmentById;

public sealed class GetHomeworkAssignmentByIdQuery : IQuery<GetHomeworkAssignmentByIdResponse>
{
    public Guid Id { get; init; }
}

