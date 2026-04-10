using Kidzgo.Application.TeachingMaterials.CreateTeachingMaterialBookmark;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialBookmarks;

public sealed class GetTeachingMaterialBookmarksResponse
{
    public Page<TeachingMaterialBookmarkResponse> Bookmarks { get; init; } = null!;
}
