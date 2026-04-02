using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Users.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Authentication.Profiles.GetProfiles;

public sealed class GetProfilesQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetProfilesQuery, List<GetProfilesResponse>>
{
    public async Task<Result<List<GetProfilesResponse>>> Handle(GetProfilesQuery request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;
        var now = DateTime.UtcNow;

        List<Profile> profiles = await context.Profiles
            .Include(p => p.User)
            .Where(p => p.UserId == userId && !p.IsDeleted && p.IsActive)
            .ToListAsync(cancellationToken);

        var response = profiles
            .Select(p => new GetProfilesResponse
            {
                Id = p.Id,
                DisplayName = p.DisplayName,
                ProfileType = p.ProfileType.ToString(),
                LastLoginAt = p.ProfileType == ProfileType.Parent ? p.User.LastLoginAt : p.LastLoginAt,
                LastSeenAt = p.ProfileType == ProfileType.Parent ? p.User.LastSeenAt : p.LastSeenAt,
                IsOnline = UserPresenceHelper.IsOnline(
                    p.ProfileType == ProfileType.Parent ? p.User.LastSeenAt : p.LastSeenAt,
                    now),
                OfflineDurationSeconds = UserPresenceHelper.GetOfflineDurationSeconds(
                    p.ProfileType == ProfileType.Parent ? p.User.LastSeenAt : p.LastSeenAt,
                    now)
            })
            .ToList();

        return response;
    }
}


