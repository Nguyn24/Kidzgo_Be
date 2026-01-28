using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Profiles.GetProfiles;

public sealed class GetProfilesQuery : IQuery<List<GetProfilesResponse>>
{
    public string? ProfileType { get; init; }
}



































