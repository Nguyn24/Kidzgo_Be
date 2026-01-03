using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Programs.GetProgramById;

public sealed class GetProgramByIdQuery : IQuery<GetProgramByIdResponse>
{
    public Guid Id { get; init; }
}

