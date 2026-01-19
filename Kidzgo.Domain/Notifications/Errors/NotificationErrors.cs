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
}

