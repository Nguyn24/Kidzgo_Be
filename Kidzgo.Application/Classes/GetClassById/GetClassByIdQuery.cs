using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Classes.GetClassById;

public sealed class GetClassByIdQuery : IQuery<GetClassByIdResponse>
{
    public Guid Id { get; init; }
}

