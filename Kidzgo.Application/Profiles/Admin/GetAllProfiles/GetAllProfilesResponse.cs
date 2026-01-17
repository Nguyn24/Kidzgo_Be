using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Profiles.Admin.GetAllProfiles;

public sealed class GetAllProfilesResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = null!;
    public ProfileType ProfileType { get; set; }
    public string DisplayName { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

