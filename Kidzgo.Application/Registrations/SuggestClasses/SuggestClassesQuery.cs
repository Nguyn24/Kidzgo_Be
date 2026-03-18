using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Registrations.SuggestClasses;

public sealed class SuggestClassesQuery : IQuery<SuggestClassesResponse>
{
    public Guid RegistrationId { get; init; }
}
