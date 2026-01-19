using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
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

        // AvatarUrl - TODO: Add AvatarUrl field to User entity if needed
        // For now, we just update the timestamp if any field was updated

        // Update profiles if provided
        if (command.Profiles != null && command.Profiles.Any())
        {
            foreach (var profileUpdate in command.Profiles)
            {
                var profile = user.Profiles.FirstOrDefault(p => p.Id == profileUpdate.Id);
                
                if (profile != null)
                {
                    // Only update DisplayName if provided, otherwise keep existing value
                    if (!string.IsNullOrWhiteSpace(profileUpdate.DisplayName))
                    {
                        profile.DisplayName = profileUpdate.DisplayName;
                        profile.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }
        }

        user.UpdatedAt = DateTime.UtcNow;
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
            AvatarUrl = null, // TODO: Add AvatarUrl to User entity if needed
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Profiles = user.Profiles.Select(p => new ProfileDto
            {
                Id = p.Id,
                DisplayName = p.DisplayName,
                ProfileType = p.ProfileType.ToString()
            }).ToList()
        };

        return Result.Success(response);
    }
}

