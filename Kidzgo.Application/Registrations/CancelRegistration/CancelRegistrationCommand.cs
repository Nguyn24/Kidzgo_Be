using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Registrations.CancelRegistration;

public sealed class CancelRegistrationCommand : ICommand<CancelRegistrationResponse>
{
    public Guid Id { get; init; }
    public string? Reason { get; init; }
}
