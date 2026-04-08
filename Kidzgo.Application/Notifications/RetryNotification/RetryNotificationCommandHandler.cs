using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Notifications.Errors;
using Kidzgo.Domain.Notifications.Events;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Notifications.RetryNotification;

public sealed class RetryNotificationCommandHandler(
    IDbContext context
) : ICommandHandler<RetryNotificationCommand, RetryNotificationResponse>
{
    public async Task<Result<RetryNotificationResponse>> Handle(
        RetryNotificationCommand command,
        CancellationToken cancellationToken)
    {
        var notification = await context.Notifications
            .Include(n => n.RecipientUser)
            .FirstOrDefaultAsync(n => n.Id == command.NotificationId, cancellationToken);

        if (notification is null)
        {
            return Result.Failure<RetryNotificationResponse>(
                NotificationErrors.NotFound(command.NotificationId));
        }

        // Only retry failed notifications
        if (notification.Status != NotificationStatus.Failed)
        {
            return Result.Failure<RetryNotificationResponse>(
                Error.Validation(
                    "Notification.InvalidStatus",
                    $"Cannot retry notification with status: {notification.Status}. Only failed notifications can be retried."));
        }

        // Reset status to Pending and raise domain event to trigger email sending
        notification.Status = NotificationStatus.Pending;
        notification.SentAt = null;

        // Raise domain event to trigger email sending handler
        notification.Raise(new NotificationCreatedDomainEvent(notification.Id, notification.Channel));

        await context.SaveChangesAsync(cancellationToken);

        return new RetryNotificationResponse
        {
            NotificationId = notification.Id,
            Status = notification.Status.ToString(),
            RetriedAt = VietnamTime.UtcNow()
        };
    }
}

