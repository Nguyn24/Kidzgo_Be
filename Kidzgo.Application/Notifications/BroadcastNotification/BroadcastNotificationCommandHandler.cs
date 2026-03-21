using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Notifications.Errors;
using Kidzgo.Domain.Notifications.Events;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Notifications.BroadcastNotification;

public sealed class BroadcastNotificationCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<BroadcastNotificationCommand, BroadcastNotificationResponse>
{
    public async Task<Result<BroadcastNotificationResponse>> Handle(
        BroadcastNotificationCommand command,
        CancellationToken cancellationToken)
    {
        var sender = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        var senderRole = sender?.Role.ToString();
        var senderName = sender?.Name;
        var targetRole = string.IsNullOrWhiteSpace(command.Role) ? null : command.Role;

        // Determine recipients based on filters
        var recipientUserIds = new HashSet<Guid>();

        // Filter by specific user IDs (highest priority)
        if (command.UserIds != null && command.UserIds.Any())
        {
            foreach (var userId in command.UserIds)
            {
                recipientUserIds.Add(userId);
            }
        }
        // Filter by specific profile IDs
        else if (command.ProfileIds != null && command.ProfileIds.Any())
        {
            var userIdsFromProfiles = await context.Profiles
                .Where(p => command.ProfileIds.Contains(p.Id))
                .Select(p => p.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);
            foreach (var userId in userIdsFromProfiles)
            {
                recipientUserIds.Add(userId);
            }
        }
        // Filter by student profile
        else if (command.StudentProfileId.HasValue)
        {
            var profile = await context.Profiles
                .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId.Value, cancellationToken);
            if (profile != null)
            {
                recipientUserIds.Add(profile.UserId);
            }
        }
        // Filter by class
        else if (command.ClassId.HasValue)
        {
            var userIdsFromClass = await context.ClassEnrollments
                .Where(ce => ce.ClassId == command.ClassId.Value && ce.Status == Domain.Classes.EnrollmentStatus.Active)
                .Select(ce => ce.StudentProfile.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);
            foreach (var userId in userIdsFromClass)
            {
                recipientUserIds.Add(userId);
            }
        }
        // Filter by branch
        else if (command.BranchId.HasValue)
        {
            var userIdsFromBranch = await context.Users
                .Where(u => u.BranchId == command.BranchId.Value)
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);
            foreach (var userId in userIdsFromBranch)
            {
                recipientUserIds.Add(userId);
            }
        }
        // Filter by role (supports multiple roles like "Admin,Staff" or "Parent+Student")
        else if (!string.IsNullOrWhiteSpace(command.Role))
        {
            // Parse roles - handle both comma and plus separators
            var roleParts = command.Role.Split(new[] { ',', '+' }, StringSplitOptions.RemoveEmptyEntries);
            var roles = roleParts.Select(r => r.Trim()).ToList();

            // Track which ProfileTypes we need to query
            var profileTypes = new List<ProfileType>();
            var userRoles = new List<UserRole>();

            foreach (var role in roles)
            {
                // Map string role to UserRole or ProfileType
                switch (role.ToLowerInvariant())
                {
                    case "admin":
                        userRoles.Add(UserRole.Admin);
                        break;
                    case "managementstaff":
                    case "management":
                    case "staff":
                    case "management_staff":
                        userRoles.Add(UserRole.ManagementStaff);
                        break;
                    case "accountantstaff":
                    case "accountant":
                        userRoles.Add(UserRole.AccountantStaff);
                        break;
                    case "teacher":
                    case "giaovien":
                    case "gv":
                        userRoles.Add(UserRole.Teacher);
                        break;
                    case "parent":
                    case "phuhuynh":
                    case "phụ huynh":
                        // Parent can be in both User.Role and Profile.ProfileType
                        userRoles.Add(UserRole.Parent);
                        profileTypes.Add(ProfileType.Parent);
                        break;
                    case "student":
                    case "hocsinh":
                    case "học sinh":
                    case "hs":
                        // Student only exists in Profile.ProfileType
                        profileTypes.Add(ProfileType.Student);
                        break;
                }
            }

            // Query Users by UserRole
            if (userRoles.Any())
            {
                var userIdsFromRoles = await context.Users
                    .Where(u => userRoles.Contains(u.Role))
                    .Select(u => u.Id)
                    .ToListAsync(cancellationToken);
                foreach (var userId in userIdsFromRoles)
                {
                    recipientUserIds.Add(userId);
                }
            }

            // Query Users via Profile by ProfileType
            if (profileTypes.Any())
            {
                var userIdsFromProfiles = await context.Profiles
                    .Where(p => profileTypes.Contains(p.ProfileType))
                    .Select(p => p.UserId)
                    .Distinct()
                    .ToListAsync(cancellationToken);
                foreach (var userId in userIdsFromProfiles)
                {
                    recipientUserIds.Add(userId);
                }
            }
        }
        else
        {
            // No filters specified
            return Result.Failure<BroadcastNotificationResponse>(NotificationErrors.InvalidFilters);
        }

        if (recipientUserIds.Count == 0)
        {
            return Result.Failure<BroadcastNotificationResponse>(NotificationErrors.NoRecipients);
        }

        // Create notifications for each recipient
        var now = DateTime.UtcNow;
        var notifications = new List<Notification>();
        var createdIds = new List<Guid>();

        // Get profile mappings
        var profileUserMap = new Dictionary<Guid, Guid>();
        
        // If ProfileIds provided, map userId -> profileId
        if (command.ProfileIds != null && command.ProfileIds.Any())
        {
            var profiles = await context.Profiles
                .Where(p => command.ProfileIds.Contains(p.Id))
                .Select(p => new { p.Id, p.UserId })
                .ToListAsync(cancellationToken);
            
            foreach (var profile in profiles)
            {
                if (!profileUserMap.ContainsKey(profile.UserId))
                {
                    profileUserMap[profile.UserId] = profile.Id;
                }
            }
        }
        
        // If StudentProfileId provided, map that specific profile
        if (command.StudentProfileId.HasValue)
        {
            var profile = await context.Profiles
                .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId.Value, cancellationToken);
            if (profile != null)
            {
                profileUserMap[profile.UserId] = profile.Id;
            }
        }

        foreach (var userId in recipientUserIds)
        {
            var notificationId = Guid.NewGuid();
            var notification = new Notification
            {
                Id = notificationId,
                RecipientUserId = userId,
                RecipientProfileId = profileUserMap.TryGetValue(userId, out var profileId) ? profileId : null,
                Channel = command.Channel,
                Title = command.Title,
                Content = command.Content,
                Deeplink = command.Deeplink,
                Status = NotificationStatus.Pending,
                CreatedAt = now,
                SenderRole = senderRole,
                SenderName = senderName,
                TargetRole = targetRole
            };

            // Raise domain event to trigger email/push/zalo sending
            if (command.Channel == NotificationChannel.Email || 
                command.Channel == NotificationChannel.Push ||
                command.Channel == NotificationChannel.ZaloOa)
            {
                notification.Raise(new NotificationCreatedDomainEvent(notificationId, command.Channel));
            }

            notifications.Add(notification);
            createdIds.Add(notificationId);
        }

        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync(cancellationToken);

        return new BroadcastNotificationResponse
        {
            CreatedCount = notifications.Count,
            CreatedNotificationIds = createdIds
        };
    }
}

