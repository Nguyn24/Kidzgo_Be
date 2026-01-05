using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Programs.DeleteProgram;

public sealed class DeleteProgramCommand : ICommand
{
    public Guid Id { get; init; }
}

