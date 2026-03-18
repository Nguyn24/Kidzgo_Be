using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Registrations.GetRegistrationById;

public sealed class GetRegistrationByIdQuery : IQuery<GetRegistrationByIdResponse>
{
    public Guid Id { get; init; }
}
