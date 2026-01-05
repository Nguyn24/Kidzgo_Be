using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Blogs.UnpublishBlog;

public sealed class UnpublishBlogCommand : ICommand<UnpublishBlogResponse>
{
    public Guid Id { get; init; }
}

