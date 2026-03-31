using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Registrations.TransferClass;

public sealed class TransferClassCommand : ICommand<TransferClassResponse>
{
    public Guid RegistrationId { get; init; }
    public Guid NewClassId { get; init; }
    public DateTime EffectiveDate { get; init; }
    public string Track { get; init; } = "primary";
}
