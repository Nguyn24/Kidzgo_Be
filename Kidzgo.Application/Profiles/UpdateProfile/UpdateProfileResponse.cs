using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Profiles.UpdateProfile;

public sealed class UpdateProfileResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public ProfileType ProfileType { get; init; }
    public string DisplayName { get; init; } = null!;
    public bool IsActive { get; init; }
    public bool IsDeleted { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

