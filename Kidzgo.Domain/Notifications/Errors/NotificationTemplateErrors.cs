using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Notifications.Errors;

public static class NotificationTemplateErrors
{
    public static Error NotFound(Guid? templateId) => Error.NotFound(
        "NotificationTemplate.NotFound",
        $"Notification Template with Id = '{templateId}' was not found");

    public static Error CodeAlreadyExists(string code) => Error.Conflict(
        "NotificationTemplate.CodeAlreadyExists",
        $"Notification Template with Code = '{code}' already exists");

    public static readonly Error Deleted = Error.Conflict(
        "NotificationTemplate.Deleted",
        "Cannot update a deleted notification template");

    public static readonly Error AlreadyDeleted = Error.Conflict(
        "NotificationTemplate.AlreadyDeleted",
        "Notification template is already deleted");
}

