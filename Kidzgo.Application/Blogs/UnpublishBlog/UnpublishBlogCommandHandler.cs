using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Media.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Blogs.UnpublishBlog;

public sealed class UnpublishBlogCommandHandler(
    IDbContext context
) : ICommandHandler<UnpublishBlogCommand, UnpublishBlogResponse>
{
    public async Task<Result<UnpublishBlogResponse>> Handle(UnpublishBlogCommand command, CancellationToken cancellationToken)
    {
        var blog = await context.Blogs
            .Include(b => b.CreatedByUser)
            .FirstOrDefaultAsync(b => b.Id == command.Id, cancellationToken);

        if (blog is null)
        {
            return Result.Failure<UnpublishBlogResponse>(BlogErrors.NotFound(command.Id));
        }

        if (blog.IsDeleted)
        {
            return Result.Failure<UnpublishBlogResponse>(BlogErrors.Deleted);
        }

        if (!blog.IsPublished)
        {
            return Result.Failure<UnpublishBlogResponse>(BlogErrors.NotPublished);
        }

        blog.IsPublished = false;
        blog.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new UnpublishBlogResponse
        {
            Id = blog.Id,
            Title = blog.Title,
            IsPublished = blog.IsPublished,
            UpdatedAt = blog.UpdatedAt
        };
    }
}

