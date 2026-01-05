using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Blogs.GetBlogs;

public sealed class GetBlogsQueryHandler(
    IDbContext context
) : IQueryHandler<GetBlogsQuery, GetBlogsResponse>
{
    public async Task<Result<GetBlogsResponse>> Handle(GetBlogsQuery query, CancellationToken cancellationToken)
    {
        var blogsQuery = context.Blogs
            .Include(b => b.CreatedByUser)
            .AsQueryable();

        // Filter by created by
        if (query.CreatedBy.HasValue)
        {
            blogsQuery = blogsQuery.Where(b => b.CreatedBy == query.CreatedBy.Value);
        }

        // Filter by published status
        if (query.IsPublished.HasValue)
        {
            blogsQuery = blogsQuery.Where(b => b.IsPublished == query.IsPublished.Value);
        }

        // Filter deleted
        if (!query.IncludeDeleted.GetValueOrDefault())
        {
            blogsQuery = blogsQuery.Where(b => !b.IsDeleted);
        }

        // Get total count
        int totalCount = await blogsQuery.CountAsync(cancellationToken);

        // Apply pagination
        var blogs = await blogsQuery
            .OrderByDescending(b => b.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(b => new BlogDto
            {
                Id = b.Id,
                Title = b.Title,
                Summary = b.Summary,
                Content = b.Content,
                FeaturedImageUrl = b.FeaturedImageUrl,
                CreatedBy = b.CreatedBy,
                CreatedByName = b.CreatedByUser.Name,
                IsPublished = b.IsPublished,
                IsDeleted = b.IsDeleted,
                PublishedAt = b.PublishedAt,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        var page = new Page<BlogDto>(
            blogs,
            query.PageNumber,
            query.PageSize,
            totalCount);

        return new GetBlogsResponse
        {
            Blogs = page
        };
    }
}

