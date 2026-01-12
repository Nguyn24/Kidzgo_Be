namespace Kidzgo.Application.Users.Admin.AssignBranch;

public sealed class AssignBranchResponse
{
    public Guid UserId { get; init; }
    public Guid? BranchId { get; init; }
    public string? BranchName { get; init; }
}

