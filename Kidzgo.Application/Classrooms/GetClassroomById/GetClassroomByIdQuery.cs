using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Classrooms.GetClassroomById;

public sealed class GetClassroomByIdQuery : IQuery<GetClassroomByIdResponse>
{
    public Guid Id { get; init; }
}

