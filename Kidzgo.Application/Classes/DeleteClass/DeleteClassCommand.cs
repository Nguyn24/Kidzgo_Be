using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Classes.DeleteClass;

public sealed class DeleteClassCommand : ICommand
{
    public Guid Id { get; init; }
}

