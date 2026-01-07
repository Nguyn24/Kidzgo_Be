using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Users.Admin.GetUserById;

public sealed class GetUserByIdQuery : IQuery<GetUserByIdResponse>
{
    public Guid Id { get; init; }
}

