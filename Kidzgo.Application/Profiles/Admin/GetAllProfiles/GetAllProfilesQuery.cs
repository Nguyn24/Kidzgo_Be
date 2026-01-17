using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Profiles.Admin.GetAllProfiles;

public sealed class GetAllProfilesQuery : IPageableQuery, IQuery<Page<GetAllProfilesResponse>>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public Guid? UserId { get; init; }
    public string? ProfileType { get; init; }
    public bool? IsActive { get; init; }
}

