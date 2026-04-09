using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TeachingMaterials.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.TeachingMaterials;
using Kidzgo.Domain.TeachingMaterials.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.CreateTeachingMaterialBookmark;

public sealed class CreateTeachingMaterialBookmarkCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateTeachingMaterialBookmarkCommand, TeachingMaterialBookmarkResponse>
{
    public async Task<Result<TeachingMaterialBookmarkResponse>> Handle(
        CreateTeachingMaterialBookmarkCommand command,
        CancellationToken cancellationToken)
    {
        var material = await context.TeachingMaterials
            .Include(tm => tm.Program)
            .FirstOrDefaultAsync(tm => tm.Id == command.TeachingMaterialId, cancellationToken);

        if (material is null)
        {
            return Result.Failure<TeachingMaterialBookmarkResponse>(
                TeachingMaterialErrors.NotFound(command.TeachingMaterialId));
        }

        if (!await TeachingMaterialAccessHelper.CanReadAsync(context, userContext, material, cancellationToken))
        {
            return Result.Failure<TeachingMaterialBookmarkResponse>(
                TeachingMaterialErrors.AccessDenied(command.TeachingMaterialId));
        }

        var bookmark = await context.TeachingMaterialBookmarks
            .FirstOrDefaultAsync(
                item => item.TeachingMaterialId == material.Id && item.UserId == userContext.UserId,
                cancellationToken);

        if (bookmark is null)
        {
            bookmark = new TeachingMaterialBookmark
            {
                Id = Guid.NewGuid(),
                TeachingMaterialId = material.Id,
                UserId = userContext.UserId,
                CreatedAt = DateTime.UtcNow
            };

            context.TeachingMaterialBookmarks.Add(bookmark);
        }

        bookmark.Note = NormalizeNote(command.Note);
        await context.SaveChangesAsync(cancellationToken);

        return Map(bookmark, material);
    }

    internal static TeachingMaterialBookmarkResponse Map(TeachingMaterialBookmark bookmark, TeachingMaterial material) =>
        new()
        {
            BookmarkId = bookmark.Id,
            MaterialId = material.Id,
            DisplayName = material.DisplayName,
            FileType = material.FileType.ToString(),
            ProgramName = material.Program.Name,
            UnitNumber = material.UnitNumber,
            LessonNumber = material.LessonNumber,
            Note = bookmark.Note,
            CreatedAt = bookmark.CreatedAt
        };

    private static string? NormalizeNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return null;
        }

        return note.Trim().Length <= 500 ? note.Trim() : note.Trim()[..500];
    }
}
