using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Users.Admin.GetUserById;

public sealed class GetUserByIdResponse
{
    public Guid Id { get; set; }
    public string? Username { get; set; }
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string? TeacherCompensationType { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public string? BranchAddress { get; set; }
    public string? BranchContactPhone { get; set; }
    public string? BranchContactEmail { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public bool IsOnline { get; set; }
    public long? OfflineDurationSeconds { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<UserProfilePresenceDto> Profiles { get; set; } = new();
}

public sealed class UserProfilePresenceDto
{
    public Guid Id { get; set; }
    public string ProfileType { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public bool IsOnline { get; set; }
    public long? OfflineDurationSeconds { get; set; }
    public DateTime CreatedAt { get; set; }
}

