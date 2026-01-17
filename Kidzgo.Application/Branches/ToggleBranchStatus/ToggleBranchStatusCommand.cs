using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Branches.ToggleBranchStatus;

public sealed class ToggleBranchStatusCommand : ICommand<ToggleBranchStatusResponse>
{
    public Guid Id { get; init; }
    public bool IsActive { get; init; }
}

