using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Blogs.DeleteBlog;

public sealed class DeleteBlogCommand : ICommand<DeleteBlogResponse>
{
    public Guid Id { get; init; }
}

