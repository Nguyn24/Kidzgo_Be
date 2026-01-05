using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Users.Errors;

public static class ProfileErrors
{
    public static readonly Error Invalid = Error.Problem(
        "Profile.Invalid",
        "Profile is invalid.");
}

