using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Users.Admin.GetAllUser;

public sealed class GetUsersResponse
{
    public Guid Id { get; set; }
    public string? Username { get; set; }
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<UserProfileDto> Profiles { get; set; } = new();
}

public sealed class UserProfileDto
{
    public Guid Id { get; set; }
    public string ProfileType { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}


