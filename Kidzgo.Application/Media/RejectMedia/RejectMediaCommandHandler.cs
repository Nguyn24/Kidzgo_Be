using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Media;
using Kidzgo.Domain.Media.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Media.RejectMedia;

public sealed class RejectMediaCommandHandler(
    IDbContext context
) : ICommandHandler<RejectMediaCommand, RejectMediaResponse>
{
    public async Task<Result<RejectMediaResponse>> Handle(RejectMediaCommand command, CancellationToken cancellationToken)
    {
        var media = await context.MediaAssets
            .FirstOrDefaultAsync(m => m.Id == command.Id && !m.IsDeleted, cancellationToken);

        if (media is null)
        {
            return Result.Failure<RejectMediaResponse>(
                MediaErrors.NotFound(command.Id));
        }

        if (media.ApprovalStatus == ApprovalStatus.Rejected)
        {
            return Result.Failure<RejectMediaResponse>(
                MediaErrors.AlreadyRejected);
        }

        media.ApprovalStatus = ApprovalStatus.Rejected;
        media.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return new RejectMediaResponse
        {
            Id = media.Id,
            ApprovalStatus = media.ApprovalStatus,
            UpdatedAt = media.UpdatedAt
        };
    }
}

