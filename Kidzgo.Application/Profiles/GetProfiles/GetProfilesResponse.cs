using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Profiles.GetProfiles;

public sealed class GetProfilesResponse
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = null!;
    public ProfileType ProfileType { get; set; }
}


















