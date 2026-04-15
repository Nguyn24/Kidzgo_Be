using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Users.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Profiles.GetProfiles;

public sealed class GetProfilesQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetProfilesQuery, List<GetProfilesResponse>>
{
    public async Task<Result<List<GetProfilesResponse>>> Handle(GetProfilesQuery request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;
        var now = VietnamTime.UtcNow();

        var query = context.Profiles
            .Include(p => p.User)
            .Where(p => p.UserId == userId && !p.IsDeleted && p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.ProfileType) &&
            Enum.TryParse<ProfileType>(request.ProfileType, true, out var profileType))
        {
            query = query.Where(p => p.ProfileType == profileType);
        }

        List<Profile> profiles = await query.ToListAsync(cancellationToken);

        var response = profiles
            .Select(p => new GetProfilesResponse
            {
                Id = p.Id,
                DisplayName = p.DisplayName,
                AvatarUrl = p.AvatarUrl ?? p.User.AvatarUrl,
                Name = p.Name,
                Gender = p.Gender?.ToString(),
                DateOfBirth = p.DateOfBirth,
                ProfileType = p.ProfileType.ToString(),
                IsApproved = p.IsApproved,
                LastLoginAt = p.ProfileType == ProfileType.Parent ? p.User.LastLoginAt : p.LastLoginAt,
                LastSeenAt = p.ProfileType == ProfileType.Parent ? p.User.LastSeenAt : p.LastSeenAt,
                IsOnline = UserPresenceHelper.IsOnline(
                    p.ProfileType == ProfileType.Parent ? p.User.LastSeenAt : p.LastSeenAt,
                    now),
                OfflineDurationSeconds = UserPresenceHelper.GetOfflineDurationSeconds(
                    p.ProfileType == ProfileType.Parent ? p.User.LastSeenAt : p.LastSeenAt,
                    now),
            })
            .ToList();

        return response;
    }
}





















