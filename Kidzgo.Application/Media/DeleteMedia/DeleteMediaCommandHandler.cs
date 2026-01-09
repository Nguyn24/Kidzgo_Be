using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Media.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Media.DeleteMedia;

public sealed class DeleteMediaCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteMediaCommand>
{
    public async Task<Result> Handle(DeleteMediaCommand command, CancellationToken cancellationToken)
    {
        var media = await context.MediaAssets
            .FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);

        if (media is null)
        {
            return Result.Failure(
                MediaErrors.NotFound(command.Id));
        }

        if (media.IsDeleted)
        {
            return Result.Failure(
                MediaErrors.AlreadyDeleted);
        }

        media.IsDeleted = true;
        media.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

