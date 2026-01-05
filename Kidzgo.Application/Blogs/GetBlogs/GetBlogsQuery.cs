using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Blogs.GetBlogs;

public sealed class GetBlogsQuery : IQuery<GetBlogsResponse>, IPageableQuery
{
    public Guid? CreatedBy { get; init; }
    public bool? IsPublished { get; init; }
    public bool? IncludeDeleted { get; init; } = false;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

