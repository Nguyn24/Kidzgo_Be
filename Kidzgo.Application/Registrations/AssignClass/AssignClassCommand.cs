using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Registrations.AssignClass;

public sealed class AssignClassCommand : ICommand<AssignClassResponse>
{
    public Guid RegistrationId { get; init; }
    public Guid ClassId { get; init; }
    public string EntryType { get; init; } = "immediate"; // "immediate" | "makeup" | "wait"
}