using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;

namespace Kidzgo.Application.Classes.ChangeClassStatus;

public sealed class ChangeClassStatusCommand : ICommand<ChangeClassStatusResponse>
{
    public Guid Id { get; init; }
    public ClassStatus Status { get; init; }
}

