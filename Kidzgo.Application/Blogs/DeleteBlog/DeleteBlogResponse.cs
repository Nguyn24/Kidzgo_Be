namespace Kidzgo.Application.Blogs.DeleteBlog;

public sealed class DeleteBlogResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public bool IsDeleted { get; init; }
    public DateTime UpdatedAt { get; init; }
}

