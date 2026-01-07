using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Users.Admin.GetAllUser;

public sealed class GetUsersResponse
{
    public Guid Id { get; set; }
    public string? Username { get; set; }
    public string? Name { get; set; }
    public string Email { get; set; } = null!;
    public UserRole Role { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}


