using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Programs.ToggleProgramStatus;

public sealed class ToggleProgramStatusCommand : ICommand<ToggleProgramStatusResponse>
{
    public Guid Id { get; init; }
}

