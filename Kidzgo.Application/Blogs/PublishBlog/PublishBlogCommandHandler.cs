using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Blogs.PublishBlog;

public sealed class PublishBlogCommandHandler(
    IDbContext context
) : ICommandHandler<PublishBlogCommand, PublishBlogResponse>
{
    public async Task<Result<PublishBlogResponse>> Handle(PublishBlogCommand command, CancellationToken cancellationToken)
    {
        var blog = await context.Blogs
            .Include(b => b.CreatedByUser)
            .FirstOrDefaultAsync(b => b.Id == command.Id, cancellationToken);

        if (blog is null)
        {
            return Result.Failure<PublishBlogResponse>(
                Error.NotFound("Blog.NotFound", "Blog not found"));
        }

        if (blog.IsDeleted)
        {
            return Result.Failure<PublishBlogResponse>(
                Error.Conflict("Blog.Deleted", "Cannot publish a deleted blog"));
        }

        if (blog.IsPublished)
        {
            return Result.Failure<PublishBlogResponse>(
                Error.Conflict("Blog.AlreadyPublished", "Blog is already published"));
        }

        var now = DateTime.UtcNow;
        blog.IsPublished = true;
        blog.PublishedAt = now;
        blog.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return new PublishBlogResponse
        {
            Id = blog.Id,
            Title = blog.Title,
            IsPublished = blog.IsPublished,
            PublishedAt = blog.PublishedAt,
            UpdatedAt = blog.UpdatedAt
        };
    }
}

