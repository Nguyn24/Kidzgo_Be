using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Branches.DeleteBranch;

public sealed class DeleteBranchCommand : ICommand
{
    public Guid Id { get; init; }
}

