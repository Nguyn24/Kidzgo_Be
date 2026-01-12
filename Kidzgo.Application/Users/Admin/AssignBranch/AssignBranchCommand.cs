using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Users.Admin.AssignBranch;

public sealed class AssignBranchCommand : ICommand<AssignBranchResponse>
{
    public Guid UserId { get; init; }
    public Guid? BranchId { get; init; }
}

