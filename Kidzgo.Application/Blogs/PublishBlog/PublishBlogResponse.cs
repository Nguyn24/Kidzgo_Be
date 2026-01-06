namespace Kidzgo.Application.Blogs.PublishBlog;

public sealed class PublishBlogResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public bool IsPublished { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

