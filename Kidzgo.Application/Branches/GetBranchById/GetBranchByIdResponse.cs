namespace Kidzgo.Application.Branches.GetBranchById;

public sealed class GetBranchByIdResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

