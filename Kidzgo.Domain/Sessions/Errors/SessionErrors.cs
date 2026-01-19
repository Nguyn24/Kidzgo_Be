using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Sessions.Errors;

public static class SessionErrors
{
    public static Error NotFound(Guid? sessionId) => Error.NotFound(
        "Session.NotFound",
        $"Session with Id = '{sessionId}' was not found");

    public static Error InvalidStatus => Error.Validation(
        "Session.InvalidStatus",
        "Only sessions with Scheduled status can be updated");

    public static Error InvalidClassStatus => Error.Validation(
        "Session.InvalidClassStatus",
        "Sessions can only be created for Planned or Active classes");

    public static Error AlreadyCancelled => Error.Validation(
        "Session.AlreadyCancelled",
        "Session is already cancelled");

    public static Error Cancelled => Error.Validation(
        "Session.Cancelled",
        "Cancelled sessions cannot be completed");
}

