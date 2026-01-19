using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Notifications.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Notifications.BroadcastNotification;

public sealed class BroadcastNotificationCommandHandler(
    IDbContext context
) : ICommandHandler<BroadcastNotificationCommand, BroadcastNotificationResponse>
{
    public async Task<Result<BroadcastNotificationResponse>> Handle(
        BroadcastNotificationCommand command,
        CancellationToken cancellationToken)
    {
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
        // Filter by role
        else if (!string.IsNullOrWhiteSpace(command.Role))
        {
            var userIdsFromRole = await context.Users
                .Where(u => u.Role.ToString() == command.Role)
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);
            foreach (var userId in userIdsFromRole)
            {
                recipientUserIds.Add(userId);
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
                CreatedAt = now
            };

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

