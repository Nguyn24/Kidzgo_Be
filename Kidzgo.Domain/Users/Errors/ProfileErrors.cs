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
    
    public static Error NotFoundList(List<Guid>? profileId) => Error.NotFound(
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

    public static readonly Error EmailNotSet = Error.Validation(
        "Profile.EmailNotSet",
        "Email is required for PIN reset.");

    public static readonly Error StudentIdNotSelected = Error.NotFound(
        "Profile.StudentIdNotSelected",
        "No student selected in token");

    public static readonly Error StudentNotLinkedToParent = Error.NotFound(
        "Profile.StudentNotLinkedToParent",
        "Student not linked to this parent");

    public static readonly Error ProfileNotDeleted = Error.Validation(
        "Profile.ProfileNotDeleted",
        "Profile is not deleted and cannot be reactivated");

    public static readonly Error UserMustBeParentOrStudent = Error.Validation(
        "Profile.UserMustBeParentOrStudent",
        "User must be a parent or student");

    public static readonly Error ProfileAlreadyApproved = Error.Conflict(
        "Profile.ProfileAlreadyApproved",
        "Profile is already approved");
}

