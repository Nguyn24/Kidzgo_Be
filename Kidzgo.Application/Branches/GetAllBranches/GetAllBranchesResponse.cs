namespace Kidzgo.Application.Branches.GetAllBranches;

public sealed class GetAllBranchesResponse
{
    public List<BranchDto> Branches { get; init; } = new();
}

public sealed class BranchDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Address { get; init; }
    public string? ContactPhone { get; init; }
    public string? ContactEmail { get; init; }
    public bool IsActive { get; init; }
}

