using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Profiles.GetProfileById;

public sealed class GetProfileByIdResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = null!;
    public string ProfileType { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

