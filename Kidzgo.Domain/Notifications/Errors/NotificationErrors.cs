using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Notifications.Errors;

public static class NotificationErrors
{
    public static Error InvalidFilters => Error.Validation(
        "Notification.InvalidFilters",
        "At least one filter must be specified");

    public static Error NoRecipients => Error.Validation(
        "Notification.NoRecipients",
        "No recipients found matching the filters");

    public static Error NotFound(Guid? notificationId) => Error.NotFound(
        "Notification.NotFound",
        $"Notification with Id = '{notificationId}' was not found");

    public static readonly Error AccessDenied = Error.Problem(
        "Notification.AccessDenied",
        "You do not have permission to mark this notification as read");

    public static readonly Error AlreadyRead = Error.Conflict(
        "Notification.AlreadyRead",
        "Notification is already marked as read");
}

