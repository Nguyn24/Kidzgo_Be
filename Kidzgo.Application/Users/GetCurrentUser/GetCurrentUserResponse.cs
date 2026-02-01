namespace Kidzgo.Application.Users.GetCurrentUser
{
    public sealed record GetCurrentUserResponse
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
        public Guid? BranchId { get; set; }
        public BranchDto? Branch { get; set; }
        public List<ProfileDto> Profiles { get; set; } = new();
        public Guid? SelectedProfileId { get; set; }
        public List<string> Permissions { get; set; } = new(); // TODO: Implement permissions based on role
        public bool IsActive { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public sealed record BranchDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public bool IsActive { get; set; }
    }

    public sealed record ProfileDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = null!;
        public string ProfileType { get; set; } = null!;
    }
}
