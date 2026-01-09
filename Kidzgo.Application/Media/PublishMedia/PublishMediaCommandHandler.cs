using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Media;
using Kidzgo.Domain.Media.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Media.PublishMedia;

public sealed class PublishMediaCommandHandler(
    IDbContext context
) : ICommandHandler<PublishMediaCommand, PublishMediaResponse>
{
    public async Task<Result<PublishMediaResponse>> Handle(PublishMediaCommand command, CancellationToken cancellationToken)
    {
        var media = await context.MediaAssets
            .FirstOrDefaultAsync(m => m.Id == command.Id && !m.IsDeleted, cancellationToken);

        if (media is null)
        {
            return Result.Failure<PublishMediaResponse>(
                MediaErrors.NotFound(command.Id));
        }

        if (media.IsPublished)
        {
            return Result.Failure<PublishMediaResponse>(
                MediaErrors.AlreadyPublished);
        }

        if (media.ApprovalStatus != ApprovalStatus.Approved)
        {
            return Result.Failure<PublishMediaResponse>(
                MediaErrors.NotApproved);
        }

        media.IsPublished = true;
        media.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return new PublishMediaResponse
        {
            Id = media.Id,
            IsPublished = media.IsPublished,
            UpdatedAt = media.UpdatedAt
        };
    }
}

