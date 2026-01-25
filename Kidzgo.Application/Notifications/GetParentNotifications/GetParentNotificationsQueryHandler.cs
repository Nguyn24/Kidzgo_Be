using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.Notifications.GetNotifications;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Notifications.GetParentNotifications;

public sealed class GetParentNotificationsQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetParentNotificationsQuery, GetParentNotificationsResponse>
{
    public async Task<Result<GetParentNotificationsResponse>> Handle(
        GetParentNotificationsQuery query, 
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        // Get parent profile for current user
        var parentProfile = await context.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && 
                                     p.ProfileType == ProfileType.Parent && 
                                     p.IsActive && 
                                     !p.IsDeleted, cancellationToken);

        if (parentProfile == null)
        {
            return Result.Failure<GetParentNotificationsResponse>(
                Error.NotFound("ParentProfile", "Parent profile not found for current user"));
        }

        // Get linked student profile IDs
        var studentProfileIds = await context.ParentStudentLinks
            .AsNoTracking()
            .Where(psl => psl.ParentProfileId == parentProfile.Id)
            .Select(psl => psl.StudentProfileId)
            .ToListAsync(cancellationToken);

        // Get notifications for parent user and all linked student profiles
        var profileIds = new List<Guid?> { parentProfile.Id };
        profileIds.AddRange(studentProfileIds.Select(id => (Guid?)id));

        var notificationsQuery = context.Notifications
            .Include(n => n.RecipientUser)
            .Include(n => n.RecipientProfile)
            .Where(n => n.RecipientUserId == userId || 
                       (n.RecipientProfileId.HasValue && profileIds.Contains(n.RecipientProfileId.Value)))
            .AsQueryable();

        // Filter by unread only
        if (query.UnreadOnly == true)
        {
            notificationsQuery = notificationsQuery.Where(n => n.ReadAt == null);
        }

        // Get total count
        int totalCount = await notificationsQuery.CountAsync(cancellationToken);

        // Apply pagination
        var notifications = await notificationsQuery
            .OrderByDescending(n => n.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                RecipientUserId = n.RecipientUserId,
                RecipientProfileId = n.RecipientProfileId,
                Channel = n.Channel,
                Title = n.Title,
                Content = n.Content,
                Deeplink = n.Deeplink,
                Status = n.Status,
                SentAt = n.SentAt,
                CreatedAt = n.CreatedAt,
                IsRead = n.ReadAt != null
            })
            .ToListAsync(cancellationToken);

        var page = new Page<NotificationDto>(
            notifications,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetParentNotificationsResponse
        {
            Notifications = page
        };
    }
}

