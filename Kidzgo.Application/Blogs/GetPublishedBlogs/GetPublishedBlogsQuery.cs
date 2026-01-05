using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Blogs.GetPublishedBlogs;

public sealed class GetPublishedBlogsQuery : IQuery<GetPublishedBlogsResponse>, IPageableQuery
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

