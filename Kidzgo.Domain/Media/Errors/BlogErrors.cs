using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Media.Errors;

public static class BlogErrors
{
    public static Error NotFound(Guid? blogId) => Error.NotFound(
        "Blog.NotFound",
        $"Blog with Id = '{blogId}' was not found");

    public static readonly Error UserNotFound = Error.NotFound(
        "Blog.UserNotFound",
        "User not found or is not Admin/Staff");

    public static readonly Error Deleted = Error.Conflict(
        "Blog.Deleted",
        "Cannot update a deleted blog");

    public static readonly Error NotPublished = Error.Conflict(
        "Blog.NotPublished",
        "Blog is not published");

    public static readonly Error AlreadyPublished = Error.Conflict(
        "Blog.AlreadyPublished",
        "Blog is already published");

    public static readonly Error AlreadyDeleted = Error.Conflict(
        "Blog.AlreadyDeleted",
        "Blog is already deleted");
}

