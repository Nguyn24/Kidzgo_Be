using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Notifications.Errors;

public static class EmailTemplateErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "EmailTemplate.NotFound",
        "Email template was not found");
}

