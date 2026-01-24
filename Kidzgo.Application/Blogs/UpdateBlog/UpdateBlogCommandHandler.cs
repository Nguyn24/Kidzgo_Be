using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Media.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Blogs.UpdateBlog;

public sealed class UpdateBlogCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateBlogCommand, UpdateBlogResponse>
{
    public async Task<Result<UpdateBlogResponse>> Handle(UpdateBlogCommand command, CancellationToken cancellationToken)
    {
        var blog = await context.Blogs
            .Include(b => b.CreatedByUser)
            .FirstOrDefaultAsync(b => b.Id == command.Id, cancellationToken);

        if (blog is null)
        {
            return Result.Failure<UpdateBlogResponse>(BlogErrors.NotFound(command.Id));
        }

        if (blog.IsDeleted)
        {
            return Result.Failure<UpdateBlogResponse>(BlogErrors.Deleted);
        }

        blog.Title = command.Title;
        blog.Summary = command.Summary;
        blog.Content = command.Content;
        blog.FeaturedImageUrl = command.FeaturedImageUrl;
        blog.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateBlogResponse
        {
            Id = blog.Id,
            Title = blog.Title,
            Summary = blog.Summary,
            Content = blog.Content,
            FeaturedImageUrl = blog.FeaturedImageUrl,
            CreatedBy = blog.CreatedBy,
            CreatedByName = blog.CreatedByUser.Name,
            IsPublished = blog.IsPublished,
            IsDeleted = blog.IsDeleted,
            PublishedAt = blog.PublishedAt,
            CreatedAt = blog.CreatedAt,
            UpdatedAt = blog.UpdatedAt
        };
    }
}

