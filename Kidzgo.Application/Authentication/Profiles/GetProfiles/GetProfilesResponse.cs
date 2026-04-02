using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Authentication.Profiles.GetProfiles;

public sealed class GetProfilesResponse
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = null!;
    public string ProfileType { get; set; } = null!;
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public bool IsOnline { get; set; }
    public long? OfflineDurationSeconds { get; set; }
}


