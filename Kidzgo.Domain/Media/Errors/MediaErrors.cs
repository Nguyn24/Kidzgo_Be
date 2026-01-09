using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Media.Errors;

public static class MediaErrors
{
    public static Error NotFound(Guid? mediaId) => Error.NotFound(
        "Media.NotFound",
        $"Media with Id = '{mediaId}' was not found");

    public static readonly Error AlreadyDeleted = Error.Conflict(
        "Media.AlreadyDeleted",
        "Media is already deleted");

    public static readonly Error AlreadyApproved = Error.Conflict(
        "Media.AlreadyApproved",
        "Media is already approved");

    public static readonly Error AlreadyRejected = Error.Conflict(
        "Media.AlreadyRejected",
        "Media is already rejected");

    public static readonly Error AlreadyPublished = Error.Conflict(
        "Media.AlreadyPublished",
        "Media is already published");

    public static readonly Error NotApproved = Error.Conflict(
        "Media.NotApproved",
        "Media must be approved before publishing");

    public static readonly Error BranchNotFound = Error.NotFound(
        "Media.BranchNotFound",
        "Branch not found or inactive");

    public static readonly Error ClassNotFound = Error.NotFound(
        "Media.ClassNotFound",
        "Class not found");

    public static readonly Error StudentNotFound = Error.NotFound(
        "Media.StudentNotFound",
        "Student profile not found or is not a student");
}

