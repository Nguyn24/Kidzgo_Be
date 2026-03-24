using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Notifications.GetBroadcastNotificationHistory;

public sealed class GetBroadcastNotificationHistoryQueryHandler(
    IDbContext context
) : IQueryHandler<GetBroadcastNotificationHistoryQuery, GetBroadcastNotificationHistoryResponse>
{
    public async Task<Result<GetBroadcastNotificationHistoryResponse>> Handle(
        GetBroadcastNotificationHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var staffRoles = new[]
        {
            UserRole.Admin.ToString(),
            UserRole.ManagementStaff.ToString()
        };

        var notificationsQuery = context.Notifications
            .AsNoTracking()
            .Where(n => n.SenderRole != null && staffRoles.Contains(n.SenderRole))
            .AsQueryable();

        if (query.Channel.HasValue)
        {
            notificationsQuery = notificationsQuery.Where(n => n.Channel == query.Channel.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SenderRole))
        {
            notificationsQuery = notificationsQuery.Where(n => n.SenderRole == query.SenderRole);
        }

        if (query.FromDate.HasValue)
        {
            notificationsQuery = notificationsQuery.Where(n => n.CreatedAt >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            notificationsQuery = notificationsQuery.Where(n => n.CreatedAt <= query.ToDate.Value);
        }

        var groupedQuery = notificationsQuery
            .GroupBy(n => new
            {
                n.CreatedAt,
                n.Channel,
                n.Title,
                n.Content,
                n.Deeplink,
                n.SenderRole,
                n.SenderName,
                n.TargetRole
            });

        var totalCount = await groupedQuery.CountAsync(cancellationToken);

        var items = await groupedQuery
            .OrderByDescending(g => g.Key.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(g => new BroadcastNotificationHistoryDto
            {
                CreatedAt = g.Key.CreatedAt,
                Channel = g.Key.Channel,
                Title = g.Key.Title,
                Content = g.Key.Content,
                Deeplink = g.Key.Deeplink,
                SenderRole = g.Key.SenderRole,
                SenderName = g.Key.SenderName,
                TargetRole = g.Key.TargetRole,
                RecipientCount = g.Count(),
                PendingCount = g.Count(n => n.Status == NotificationStatus.Pending),
                SentCount = g.Count(n => n.Status == NotificationStatus.Sent),
                FailedCount = g.Count(n => n.Status == NotificationStatus.Failed)
            })
            .ToListAsync(cancellationToken);

        var page = new Page<BroadcastNotificationHistoryDto>(
            items,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetBroadcastNotificationHistoryResponse
        {
            Broadcasts = page
        };
    }
}
