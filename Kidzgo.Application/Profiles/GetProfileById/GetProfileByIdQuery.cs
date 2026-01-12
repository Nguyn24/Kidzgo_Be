using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Profiles.GetProfileById;

public sealed class GetProfileByIdQuery : IQuery<GetProfileByIdResponse>
{
    public Guid Id { get; init; }
}

