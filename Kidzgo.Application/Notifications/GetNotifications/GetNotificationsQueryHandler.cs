using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Notifications.GetNotifications;

public sealed class GetNotificationsQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetNotificationsQuery, GetNotificationsResponse>
{
    public async Task<Result<GetNotificationsResponse>> Handle(GetNotificationsQuery query, CancellationToken cancellationToken)
    {
        var currentUserId = userContext.UserId;

        var notificationsQuery = context.Notifications
            .Include(n => n.RecipientUser)
            .Include(n => n.RecipientProfile)
            .Where(n => n.RecipientUserId == currentUserId)
            .AsQueryable();

        // Filter by profileId (if provided)
        if (query.ProfileId.HasValue)
        {
            notificationsQuery = notificationsQuery.Where(n => 
                n.RecipientProfileId == query.ProfileId.Value);
        }

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

        return new GetNotificationsResponse
        {
            Notifications = page
        };
    }
}

