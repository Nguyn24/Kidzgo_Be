using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Media;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Blogs.CreateBlog;

public sealed class CreateBlogCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateBlogCommand, CreateBlogResponse>
{
    public async Task<Result<CreateBlogResponse>> Handle(CreateBlogCommand command, CancellationToken cancellationToken)
    {
        var createdBy = userContext.UserId;

        // Check if user exists and is Admin or Staff
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == createdBy && 
                (u.Role == Domain.Users.UserRole.Admin || u.Role == Domain.Users.UserRole.Staff), cancellationToken);

        if (user is null)
        {
            return Result.Failure<CreateBlogResponse>(
                Error.NotFound("Blog.UserNotFound", "User not found or is not Admin/Staff"));
        }

        var now = DateTime.UtcNow;
        var blog = new Blog
        {
            Id = Guid.NewGuid(),
            Title = command.Title,
            Summary = command.Summary,
            Content = command.Content,
            FeaturedImageUrl = command.FeaturedImageUrl,
            CreatedBy = createdBy,
            IsPublished = false,
            IsDeleted = false,
            PublishedAt = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Blogs.Add(blog);
        await context.SaveChangesAsync(cancellationToken);

        // Query blog with navigation properties for response
        var blogWithNav = await context.Blogs
            .Include(b => b.CreatedByUser)
            .FirstOrDefaultAsync(b => b.Id == blog.Id, cancellationToken);

        return new CreateBlogResponse
        {
            Id = blogWithNav!.Id,
            Title = blogWithNav.Title,
            Summary = blogWithNav.Summary,
            Content = blogWithNav.Content,
            FeaturedImageUrl = blogWithNav.FeaturedImageUrl,
            CreatedBy = blogWithNav.CreatedBy,
            CreatedByName = blogWithNav.CreatedByUser.Name,
            IsPublished = blogWithNav.IsPublished,
            IsDeleted = blogWithNav.IsDeleted,
            PublishedAt = blogWithNav.PublishedAt,
            CreatedAt = blogWithNav.CreatedAt,
            UpdatedAt = blogWithNav.UpdatedAt
        };
    }
}

