using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Media.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Media.UpdateMedia;

public sealed class UpdateMediaCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateMediaCommand, UpdateMediaResponse>
{
    public async Task<Result<UpdateMediaResponse>> Handle(UpdateMediaCommand command, CancellationToken cancellationToken)
    {
        var media = await context.MediaAssets
            .FirstOrDefaultAsync(m => m.Id == command.Id && !m.IsDeleted, cancellationToken);

        if (media is null)
        {
            return Result.Failure<UpdateMediaResponse>(
                MediaErrors.NotFound(command.Id));
        }

        // Update fields if provided
        if (command.ClassId.HasValue)
        {
            media.ClassId = command.ClassId.Value;
        }

        if (command.StudentProfileId.HasValue)
        {
            media.StudentProfileId = command.StudentProfileId.Value;
        }

        if (!string.IsNullOrWhiteSpace(command.MonthTag))
        {
            media.MonthTag = command.MonthTag;
        }

        if (command.ContentType.HasValue)
        {
            media.ContentType = command.ContentType.Value;
        }

        if (command.Caption is not null)
        {
            media.Caption = command.Caption;
        }

        if (command.Visibility.HasValue)
        {
            media.Visibility = command.Visibility.Value;
        }

        media.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return new UpdateMediaResponse
        {
            Id = media.Id,
            ClassId = media.ClassId,
            StudentProfileId = media.StudentProfileId,
            MonthTag = media.MonthTag,
            ContentType = media.ContentType,
            Caption = media.Caption,
            Visibility = media.Visibility,
            UpdatedAt = media.UpdatedAt
        };
    }
}

