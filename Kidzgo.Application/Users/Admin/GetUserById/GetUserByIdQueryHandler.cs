using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Users.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Users.Admin.GetUserById;

public sealed class GetUserByIdQueryHandler(IDbContext context)
    : IQueryHandler<GetUserByIdQuery, GetUserByIdResponse>
{
    public async Task<Result<GetUserByIdResponse>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.Branch)
            .Include(u => u.Profiles.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(u => u.Id == query.Id, cancellationToken);

        if (user is null)
        {
            return Result.Failure<GetUserByIdResponse>(UserErrors.NotFound(query.Id));
        }

        var now = DateTime.UtcNow;

        return Result.Success(new GetUserByIdResponse
        {
            Id = user.Id,
            Username = user.Username,
            Name = user.Name,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            Role = user.Role.ToString(),
            BranchId = user.BranchId,
            BranchCode = user.Branch?.Code,
            BranchName = user.Branch?.Name,
            BranchAddress = user.Branch?.Address,
            BranchContactPhone = user.Branch?.ContactPhone,
            BranchContactEmail = user.Branch?.ContactEmail,
            IsActive = user.IsActive,
            IsDeleted = user.IsDeleted,
            LastLoginAt = user.LastLoginAt,
            LastSeenAt = user.LastSeenAt,
            IsOnline = UserPresenceHelper.IsOnline(user.LastSeenAt, now),
            OfflineDurationSeconds = UserPresenceHelper.GetOfflineDurationSeconds(user.LastSeenAt, now),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Profiles = user.Profiles
                .Where(p => !p.IsDeleted)
                .Select(p => new UserProfilePresenceDto
                {
                    Id = p.Id,
                    ProfileType = p.ProfileType.ToString(),
                    DisplayName = p.DisplayName,
                    IsActive = p.IsActive,
                    LastLoginAt = p.ProfileType == ProfileType.Parent ? user.LastLoginAt : p.LastLoginAt,
                    LastSeenAt = p.ProfileType == ProfileType.Parent ? user.LastSeenAt : p.LastSeenAt,
                    IsOnline = UserPresenceHelper.IsOnline(
                        p.ProfileType == ProfileType.Parent ? user.LastSeenAt : p.LastSeenAt,
                        now),
                    OfflineDurationSeconds = UserPresenceHelper.GetOfflineDurationSeconds(
                        p.ProfileType == ProfileType.Parent ? user.LastSeenAt : p.LastSeenAt,
                        now),
                    CreatedAt = p.CreatedAt
                })
                .ToList()
        });
    }
}

