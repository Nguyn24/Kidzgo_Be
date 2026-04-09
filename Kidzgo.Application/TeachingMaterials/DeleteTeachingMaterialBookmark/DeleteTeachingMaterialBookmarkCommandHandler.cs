using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.DeleteTeachingMaterialBookmark;

public sealed class DeleteTeachingMaterialBookmarkCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<DeleteTeachingMaterialBookmarkCommand>
{
    public async Task<Result> Handle(DeleteTeachingMaterialBookmarkCommand command, CancellationToken cancellationToken)
    {
        var bookmark = await context.TeachingMaterialBookmarks
            .FirstOrDefaultAsync(
                item => item.TeachingMaterialId == command.TeachingMaterialId && item.UserId == userContext.UserId,
                cancellationToken);

        if (bookmark is not null)
        {
            context.TeachingMaterialBookmarks.Remove(bookmark);
            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
