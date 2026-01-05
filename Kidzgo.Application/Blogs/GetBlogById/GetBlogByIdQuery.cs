using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Blogs.GetBlogById;

public sealed class GetBlogByIdQuery : IQuery<GetBlogByIdResponse>
{
    public Guid Id { get; init; }
}

