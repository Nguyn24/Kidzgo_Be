using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Blogs.PublishBlog;

public sealed class PublishBlogCommand : ICommand<PublishBlogResponse>
{
    public Guid Id { get; init; }
}

