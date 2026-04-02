using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Users.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Profiles.GetProfileById;

public sealed class GetProfileByIdQueryHandler(IDbContext context)
    : IQueryHandler<GetProfileByIdQuery, GetProfileByIdResponse>
{
    public async Task<Result<GetProfileByIdResponse>> Handle(GetProfileByIdQuery query, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var profile = await context.Profiles
            .Include(p => p.User)
            .Where(p => p.Id == query.Id)
            .Select(p => new GetProfileByIdResponse
            {
                Id = p.Id,
                UserId = p.UserId,
                UserEmail = p.User.Email,
                ProfileType = p.ProfileType.ToString(),
                DisplayName = p.DisplayName,
                Name = p.Name,
                Gender = p.Gender != null ? p.Gender.ToString() : null,
                DateOfBirth = p.DateOfBirth,
                IsActive = p.IsActive,
                IsDeleted = p.IsDeleted,
                IsApproved = p.IsApproved,
                LastLoginAt = p.ProfileType == ProfileType.Parent ? p.User.LastLoginAt : p.LastLoginAt,
                LastSeenAt = p.ProfileType == ProfileType.Parent ? p.User.LastSeenAt : p.LastSeenAt,
                IsOnline = UserPresenceHelper.IsOnline(
                    p.ProfileType == ProfileType.Parent ? p.User.LastSeenAt : p.LastSeenAt,
                    now),
                OfflineDurationSeconds = UserPresenceHelper.GetOfflineDurationSeconds(
                    p.ProfileType == ProfileType.Parent ? p.User.LastSeenAt : p.LastSeenAt,
                    now),
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return Result.Failure<GetProfileByIdResponse>(ProfileErrors.NotFound(query.Id));
        }

        return profile;
    }
}

