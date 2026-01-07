using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Users.Admin.GetUserById;

public sealed class GetUserByIdResponse
{
    public Guid Id { get; set; }
    public string? Username { get; set; }
    public string? Name { get; set; }
    public string Email { get; set; } = null!;
    public UserRole Role { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public string? BranchAddress { get; set; }
    public string? BranchContactPhone { get; set; }
    public string? BranchContactEmail { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

