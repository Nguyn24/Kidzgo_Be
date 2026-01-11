using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Notifications;
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
            return Result.Failure<MarkNotificationAsReadResponse>(
                Error.NotFound("Notification.NotFound", "Notification not found"));
        }

        // Check if notification belongs to current user
        if (notification.RecipientUserId != currentUserId)
        {
            return Result.Failure<MarkNotificationAsReadResponse>(
                Error.Problem("Notification.AccessDenied", "You do not have permission to mark this notification as read"));
        }

        // Check if already read
        if (notification.ReadAt.HasValue)
        {
            return Result.Failure<MarkNotificationAsReadResponse>(
                Error.Conflict("Notification.AlreadyRead", "Notification is already marked as read"));
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

