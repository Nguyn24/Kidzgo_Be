using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Blogs.GetPublishedBlogs;

public sealed class GetPublishedBlogsResponse
{
    public Page<PublishedBlogDto> Blogs { get; init; } = null!;
}

public sealed class PublishedBlogDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string? Summary { get; init; }
    public string? FeaturedImageUrl { get; init; }
    public Guid CreatedBy { get; init; }
    public string CreatedByName { get; init; } = null!;
    public DateTime? PublishedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

