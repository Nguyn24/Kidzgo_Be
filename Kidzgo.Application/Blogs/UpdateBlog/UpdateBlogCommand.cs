using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Blogs.UpdateBlog;

public sealed class UpdateBlogCommand : ICommand<UpdateBlogResponse>
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string? Summary { get; init; }
    public string Content { get; init; } = null!;
    public string? FeaturedImageUrl { get; init; }
}

