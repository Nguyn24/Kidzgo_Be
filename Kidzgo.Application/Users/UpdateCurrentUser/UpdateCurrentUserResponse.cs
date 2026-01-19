namespace Kidzgo.Application.Users.UpdateCurrentUser;

public sealed class UpdateCurrentUserResponse
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public Guid? BranchId { get; set; }
    public string? AvatarUrl { get; set; } // TODO: Add AvatarUrl to User entity if needed
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ProfileDto> Profiles { get; set; } = new();
}

public sealed class ProfileDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = null!;
    public string ProfileType { get; set; } = null!;
}

