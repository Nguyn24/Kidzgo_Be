namespace Kidzgo.Application.Blogs.UpdateBlog;

public sealed class UpdateBlogResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string? Summary { get; init; }
    public string Content { get; init; } = null!;
    public string? FeaturedImageUrl { get; init; }
    public Guid CreatedBy { get; init; }
    public string CreatedByName { get; init; } = null!;
    public bool IsPublished { get; init; }
    public bool IsDeleted { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

