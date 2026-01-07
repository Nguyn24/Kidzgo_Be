using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Sessions.GetSessionById;

public sealed class GetSessionByIdQuery : IQuery<GetSessionByIdResponse>
{
    public Guid SessionId { get; init; }
}

