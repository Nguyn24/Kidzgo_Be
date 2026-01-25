using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Media.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Blogs.DeleteBlog;

public sealed class DeleteBlogCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteBlogCommand, DeleteBlogResponse>
{
    public async Task<Result<DeleteBlogResponse>> Handle(DeleteBlogCommand command, CancellationToken cancellationToken)
    {
        var blog = await context.Blogs
            .Include(b => b.CreatedByUser)
            .FirstOrDefaultAsync(b => b.Id == command.Id, cancellationToken);

        if (blog is null)
        {
            return Result.Failure<DeleteBlogResponse>(BlogErrors.NotFound(command.Id));
        }

        if (blog.IsDeleted)
        {
            return Result.Failure<DeleteBlogResponse>(BlogErrors.AlreadyDeleted);
        }

        // Soft delete
        blog.IsDeleted = true;
        blog.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new DeleteBlogResponse
        {
            Id = blog.Id,
            Title = blog.Title,
            IsDeleted = blog.IsDeleted,
            UpdatedAt = blog.UpdatedAt
        };
    }
}

