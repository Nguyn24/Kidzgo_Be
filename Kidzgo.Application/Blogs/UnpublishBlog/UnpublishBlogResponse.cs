namespace Kidzgo.Application.Blogs.UnpublishBlog;

public sealed class UnpublishBlogResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public bool IsPublished { get; init; }
    public DateTime UpdatedAt { get; init; }
}

