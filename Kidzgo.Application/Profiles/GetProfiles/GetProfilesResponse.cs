using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Profiles.GetProfiles;

public sealed class GetProfilesResponse
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = null!;
    public string? Name { get; set; }
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string ProfileType { get; set; } = null!;
    public bool IsApproved { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public bool IsOnline { get; set; }
    public long? OfflineDurationSeconds { get; set; }
}





















