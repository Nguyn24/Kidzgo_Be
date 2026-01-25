using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Media.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Blogs.GetBlogById;

public sealed class GetBlogByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetBlogByIdQuery, GetBlogByIdResponse>
{
    public async Task<Result<GetBlogByIdResponse>> Handle(GetBlogByIdQuery query, CancellationToken cancellationToken)
    {
        var blog = await context.Blogs
            .Include(b => b.CreatedByUser)
            .FirstOrDefaultAsync(b => b.Id == query.Id, cancellationToken);

        if (blog is null)
        {
            return Result.Failure<GetBlogByIdResponse>(BlogErrors.NotFound(query.Id));
        }

        return new GetBlogByIdResponse
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

