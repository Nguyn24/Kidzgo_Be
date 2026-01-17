namespace Kidzgo.Application.Branches.ToggleBranchStatus;

public sealed class ToggleBranchStatusResponse
{
    public Guid Id { get; init; }
    public bool IsActive { get; init; }
}

