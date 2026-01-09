using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Sessions.Errors;

public static class SessionErrors
{
    public static Error NotFound(Guid? sessionId) => Error.NotFound(
        "Session.NotFound",
        $"Session with Id = '{sessionId}' was not found");
}

