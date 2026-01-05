using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Blogs.CreateBlog;

public sealed class CreateBlogCommand : ICommand<CreateBlogResponse>
{
    public string Title { get; init; } = null!;
    public string? Summary { get; init; }
    public string Content { get; init; } = null!;
    public string? FeaturedImageUrl { get; init; }
}

