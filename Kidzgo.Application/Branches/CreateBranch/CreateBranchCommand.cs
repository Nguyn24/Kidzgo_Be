using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Branches.CreateBranch;

public sealed class CreateBranchCommand : ICommand<CreateBranchResponse>
{
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Address { get; init; }
    public string? ContactPhone { get; init; }
    public string? ContactEmail { get; init; }
}

