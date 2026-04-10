using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.TeachingMaterials.CreateTeachingMaterialBookmark;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialBookmarks;

public sealed class GetTeachingMaterialBookmarksQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetTeachingMaterialBookmarksQuery, GetTeachingMaterialBookmarksResponse>
{
    public async Task<Result<GetTeachingMaterialBookmarksResponse>> Handle(
        GetTeachingMaterialBookmarksQuery query,
        CancellationToken cancellationToken)
    {
        var bookmarksQuery = context.TeachingMaterialBookmarks
            .AsNoTracking()
            .Include(bookmark => bookmark.TeachingMaterial)
            .ThenInclude(material => material.Program)
            .Where(bookmark => bookmark.UserId == userContext.UserId);

        var totalCount = await bookmarksQuery.CountAsync(cancellationToken);

        var bookmarks = await bookmarksQuery
            .OrderByDescending(bookmark => bookmark.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(bookmark => new TeachingMaterialBookmarkResponse
            {
                BookmarkId = bookmark.Id,
                MaterialId = bookmark.TeachingMaterialId,
                DisplayName = bookmark.TeachingMaterial.DisplayName,
                FileType = bookmark.TeachingMaterial.FileType.ToString(),
                ProgramName = bookmark.TeachingMaterial.Program.Name,
                UnitNumber = bookmark.TeachingMaterial.UnitNumber,
                LessonNumber = bookmark.TeachingMaterial.LessonNumber,
                Note = bookmark.Note,
                CreatedAt = bookmark.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new GetTeachingMaterialBookmarksResponse
        {
            Bookmarks = new Page<TeachingMaterialBookmarkResponse>(
                bookmarks,
                totalCount,
                query.PageNumber,
                query.PageSize)
        };
    }
}
