using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Notifications;
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
        var currentUserId = userContext.UserId;

        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == command.NotificationId, cancellationToken);

        if (notification is null)
        {
            return Result.Failure<MarkNotificationAsReadResponse>(NotificationErrors.NotFound(command.NotificationId));
        }

        // Check if notification belongs to current user
        if (notification.RecipientUserId != currentUserId)
        {
            return Result.Failure<MarkNotificationAsReadResponse>(NotificationErrors.AccessDenied);
        }

        // Check if already read
        if (notification.ReadAt.HasValue)
        {
            return Result.Failure<MarkNotificationAsReadResponse>(NotificationErrors.AlreadyRead);
        }

        // Mark as read
        var now = DateTime.UtcNow;
        notification.ReadAt = now;
        
        await context.SaveChangesAsync(cancellationToken);

        return new MarkNotificationAsReadResponse
        {
            Id = notification.Id,
            ReadAt = now
        };
    }
}

