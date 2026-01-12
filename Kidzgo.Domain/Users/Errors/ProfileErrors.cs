using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Users.Errors;

public static class ProfileErrors
{
    public static readonly Error Invalid = Error.Problem(
        "Profile.Invalid",
        "Profile is invalid.");
    
    public static Error NotFound(Guid? profileId) => Error.NotFound(
        "Profile.NotFound",
        $"The profile with the Id = '{profileId}' was not found");
    
    public static readonly Error DisplayNameRequired = Error.Validation(
        "Profile.DisplayNameRequired",
        "Display name is required");
    
    public static readonly Error UserNotFound = Error.NotFound(
        "Profile.UserNotFound",
        "User not found");
    
    public static readonly Error InvalidProfileType = Error.Validation(
        "Profile.InvalidProfileType",
        "Profile type must be Parent or Student");
    
    public static readonly Error StudentNotFound = Error.NotFound(
        "Profile.StudentNotFound",
        "Student profile not found");
    
    public static readonly Error ParentNotFound = Error.NotFound(
        "Profile.ParentNotFound",
        "Parent profile not found");
    
    public static readonly Error LinkAlreadyExists = Error.Conflict(
        "Profile.LinkAlreadyExists",
        "Parent-Student link already exists");
    
    public static readonly Error LinkNotFound = Error.NotFound(
        "Profile.LinkNotFound",
        "Parent-Student link not found");
}

