using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialBookmarks;

public sealed class GetTeachingMaterialBookmarksQuery : IQuery<GetTeachingMaterialBookmarksResponse>, IPageableQuery
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
