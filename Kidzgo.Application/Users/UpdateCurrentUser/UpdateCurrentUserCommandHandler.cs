using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Users.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Users.UpdateCurrentUser;

public sealed class UpdateCurrentUserCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<UpdateCurrentUserCommand, UpdateCurrentUserResponse>
{
    public async Task<Result<UpdateCurrentUserResponse>> Handle(
        UpdateCurrentUserCommand command,
        CancellationToken cancellationToken)
    {
        var currentUserId = userContext.UserId;
        var now = VietnamTime.UtcNow();

        var user = await context.Users
            .Include(u => u.Profiles.Where(p => !p.IsDeleted && p.IsActive))
            .FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);

        if (user == null)
        {
            return Result.Failure<UpdateCurrentUserResponse>(UserErrors.NotFound(currentUserId));
        }

        // Update user fields - only update if not null, otherwise keep existing value
        if (!string.IsNullOrWhiteSpace(command.FullName))
        {
            user.Name = command.FullName;
        }

        if (!string.IsNullOrWhiteSpace(command.Email))
        {
            // Check if email already exists for another user
            var emailExists = await context.Users
                .AnyAsync(u => u.Email == command.Email && u.Id != currentUserId && !u.IsDeleted, cancellationToken);

            if (emailExists)
            {
                return Result.Failure<UpdateCurrentUserResponse>(
                    UserErrors.EmailAlreadyExists(command.Email));
            }

            user.Email = command.Email;
        }

        if (!string.IsNullOrWhiteSpace(command.PhoneNumber))
        {
            var phoneLookupCandidates = PhoneNumberNormalizer.GetLookupCandidates(command.PhoneNumber);

            var phoneNumberExists = await context.Users.AnyAsync(
                u => u.Id != currentUserId &&
                     u.PhoneNumber != null &&
                     phoneLookupCandidates.Contains(
                         u.PhoneNumber
                             .Replace(" ", "")
                             .Replace("-", "")
                             .Replace(".", "")
                             .Replace("(", "")
                             .Replace(")", "")
                             .Replace("+", "")),
                cancellationToken);

            if (phoneNumberExists)
            {
                return Result.Failure<UpdateCurrentUserResponse>(UserErrors.PhoneNumberNotUnique);
            }

            user.PhoneNumber = PhoneNumberNormalizer.NormalizeVietnamesePhoneNumber(command.PhoneNumber);
        }

        if (!string.IsNullOrWhiteSpace(command.AvatarUrl))
        {
            user.AvatarUrl = command.AvatarUrl;
        }

        if (command.Profiles != null && command.Profiles.Any())
        {
            if (user.Role == UserRole.Parent)
            {
                await ApplyParentProfileUpdatesAsync(user, command.Profiles, now, cancellationToken);
            }
            else
            {
                ApplyOwnedProfileUpdates(user.Profiles, command.Profiles, now);
            }
        }

        user.UpdatedAt = now;
        await context.SaveChangesAsync(cancellationToken);

        // Reload user with profiles to return updated data
        user = await context.Users
            .Include(u => u.Branch)
            .Include(u => u.Profiles.Where(p => !p.IsDeleted && p.IsActive))
            .FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);

        if (user == null)
        {
            return Result.Failure<UpdateCurrentUserResponse>(UserErrors.NotFound(currentUserId));
        }

        var response = new UpdateCurrentUserResponse
        {
            Id = user.Id,
            UserName = user.Username,
            FullName = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            BranchId = user.BranchId,
            AvatarUrl = user.AvatarUrl,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Profiles = user.Profiles.Select(p => new ProfileDto
            {
                Id = p.Id,
                DisplayName = p.DisplayName,
                AvatarUrl = p.AvatarUrl ?? user.AvatarUrl,
                ProfileType = p.ProfileType.ToString(),
                LastLoginAt = p.ProfileType == ProfileType.Parent ? user.LastLoginAt : p.LastLoginAt,
                LastSeenAt = p.ProfileType == ProfileType.Parent ? user.LastSeenAt : p.LastSeenAt,
                IsOnline = UserPresenceHelper.IsOnline(
                    p.ProfileType == ProfileType.Parent ? user.LastSeenAt : p.LastSeenAt,
                    now),
                OfflineDurationSeconds = UserPresenceHelper.GetOfflineDurationSeconds(
                    p.ProfileType == ProfileType.Parent ? user.LastSeenAt : p.LastSeenAt,
                    now)
            }).ToList()
        };

        return Result.Success(response);
    }

    private void ApplyOwnedProfileUpdates(
        IEnumerable<Profile> profiles,
        IEnumerable<UpdateProfileDto> profileUpdates,
        DateTime now)
    {
        foreach (var profileUpdate in profileUpdates)
        {
            var profile = profiles.FirstOrDefault(p => p.Id == profileUpdate.Id);
            ApplyProfileUpdate(profile, profileUpdate, now);
        }
    }

    private async Task ApplyParentProfileUpdatesAsync(
        User user,
        IReadOnlyCollection<UpdateProfileDto> profileUpdates,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var parentProfile = user.Profiles.FirstOrDefault(p => p.ProfileType == ProfileType.Parent);
        var selectedStudentProfile = await ResolveSelectedStudentProfileAsync(user, cancellationToken);

        var parentProfileUpdate = parentProfile is null
            ? null
            : profileUpdates.FirstOrDefault(update => update.Id == parentProfile.Id);

        ApplyProfileUpdate(parentProfile, parentProfileUpdate, now);

        if (selectedStudentProfile is null)
        {
            return;
        }

        var selectedStudentUpdate = profileUpdates.FirstOrDefault(update => update.Id == selectedStudentProfile.Id);

        if (selectedStudentUpdate is null)
        {
            selectedStudentUpdate = parentProfile is null
                ? profileUpdates.FirstOrDefault()
                : profileUpdates.FirstOrDefault(update => update.Id != parentProfile.Id);
        }

        ApplyProfileUpdate(selectedStudentProfile, selectedStudentUpdate, now);
    }

    private async Task<Profile?> ResolveSelectedStudentProfileAsync(User user, CancellationToken cancellationToken)
    {
        if (!userContext.StudentId.HasValue)
        {
            return null;
        }

        var selectedStudentId = userContext.StudentId.Value;

        var ownedProfile = user.Profiles.FirstOrDefault(
            profile => profile.Id == selectedStudentId && profile.ProfileType == ProfileType.Student);

        if (ownedProfile is not null)
        {
            return ownedProfile;
        }

        return await context.ParentStudentLinks
            .Where(link => link.StudentProfileId == selectedStudentId &&
                           !link.ParentProfile.IsDeleted &&
                           link.ParentProfile.UserId == user.Id &&
                           !link.StudentProfile.IsDeleted)
            .Select(link => link.StudentProfile)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static void ApplyProfileUpdate(Profile? profile, UpdateProfileDto? profileUpdate, DateTime now)
    {
        if (profile is null || profileUpdate is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(profileUpdate.DisplayName))
        {
            profile.DisplayName = profileUpdate.DisplayName;
            profile.UpdatedAt = now;
        }
    }
}

