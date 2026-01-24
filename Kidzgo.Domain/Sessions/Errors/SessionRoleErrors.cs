using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Sessions.Errors;

public static class SessionRoleErrors
{
    public static Error NotFound(Guid? sessionRoleId) => Error.NotFound(
        "SessionRole.NotFound",
        $"Session role with Id = '{sessionRoleId}' was not found");

    public static readonly Error AlreadyExists = Error.Conflict(
        "SessionRole.Exists",
        "Session role already exists for this session and staff user");
}

