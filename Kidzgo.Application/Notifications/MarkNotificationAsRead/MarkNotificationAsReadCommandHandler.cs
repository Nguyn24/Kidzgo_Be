using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Notifications.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Notifications.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<MarkNotificationAsReadCommand, MarkNotificationAsReadResponse>
{
    public async Task<Result<MarkNotificationAsReadResponse>> Handle(
        MarkNotificationAsReadCommand command,
        CancellationToken cancellationToken)
    {
        var response = new MarkNotificationAsReadResponse();
        var currentUserId = userContext.UserId;
        var notificationIds = command.NotificationIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var notifications = await context.Notifications
            .Where(n => notificationIds.Contains(n.Id))
            .ToListAsync(cancellationToken);

        var notificationsById = notifications.ToDictionary(n => n.Id);
        var now = VietnamTime.UtcNow();

        foreach (var notificationId in notificationIds)
        {
            if (!notificationsById.TryGetValue(notificationId, out var notification))
            {
                var error = NotificationErrors.NotFound(notificationId);
                response.Errors.Add(new MarkNotificationAsReadError
                {
                    Id = notificationId,
                    Code = error.Code,
                    Message = error.Description
                });
                continue;
            }

            if (notification.RecipientUserId != currentUserId)
            {
                response.Errors.Add(new MarkNotificationAsReadError
                {
                    Id = notificationId,
                    Code = NotificationErrors.AccessDenied.Code,
                    Message = NotificationErrors.AccessDenied.Description
                });
                continue;
            }

            if (notification.ReadAt.HasValue)
            {
                response.AlreadyReadIds.Add(notificationId);
                continue;
            }

            notification.ReadAt = now;
            response.ReadIds.Add(notificationId);
        }

        if (response.ReadIds.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}

