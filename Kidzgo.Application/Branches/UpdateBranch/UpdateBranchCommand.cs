using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Branches.UpdateBranch;

public sealed class UpdateBranchCommand : ICommand<UpdateBranchResponse>
{
    public Guid Id { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Address { get; init; }
    public string? ContactPhone { get; init; }
    public string? ContactEmail { get; init; }
}

