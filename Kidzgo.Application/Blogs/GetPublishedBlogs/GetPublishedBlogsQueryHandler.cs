using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Blogs.GetPublishedBlogs;

public sealed class GetPublishedBlogsQueryHandler(
    IDbContext context
) : IQueryHandler<GetPublishedBlogsQuery, GetPublishedBlogsResponse>
{
    public async Task<Result<GetPublishedBlogsResponse>> Handle(GetPublishedBlogsQuery query, CancellationToken cancellationToken)
    {
        var blogsQuery = context.Blogs
            .Include(b => b.CreatedByUser)
            .Where(b => b.IsPublished && !b.IsDeleted)
            .AsQueryable();

        // Get total count
        int totalCount = await blogsQuery.CountAsync(cancellationToken);

        // Apply pagination
        var blogs = await blogsQuery
            .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(b => new PublishedBlogDto
            {
                Id = b.Id,
                Title = b.Title,
                Summary = b.Summary,
                FeaturedImageUrl = b.FeaturedImageUrl,
                CreatedBy = b.CreatedBy,
                CreatedByName = b.CreatedByUser.Name,
                PublishedAt = b.PublishedAt ?? b.CreatedAt,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var page = new Page<PublishedBlogDto>(
            blogs,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetPublishedBlogsResponse
        {
            Blogs = page
        };
    }
}

