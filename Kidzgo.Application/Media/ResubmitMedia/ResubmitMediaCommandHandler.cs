using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Media;
using Kidzgo.Domain.Media.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Media.ResubmitMedia;

public sealed class ResubmitMediaCommandHandler(
    IDbContext context
) : ICommandHandler<ResubmitMediaCommand, ResubmitMediaResponse>
{
    public async Task<Result<ResubmitMediaResponse>> Handle(
        ResubmitMediaCommand command,
        CancellationToken cancellationToken)
    {
        var media = await context.MediaAssets
            .FirstOrDefaultAsync(m => m.Id == command.Id && !m.IsDeleted, cancellationToken);

        if (media is null)
        {
            return Result.Failure<ResubmitMediaResponse>(
                MediaErrors.NotFound(command.Id));
        }

        if (media.ApprovalStatus != ApprovalStatus.Rejected)
        {
            return Result.Failure<ResubmitMediaResponse>(
                MediaErrors.NotRejected);
        }

        media.ApprovalStatus = ApprovalStatus.Pending;
        media.ApprovedById = null;
        media.ApprovedAt = null;
        media.RejectReason = null;
        media.IsPublished = false;
        media.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return new ResubmitMediaResponse
        {
            Id = media.Id,
            ApprovalStatus = media.ApprovalStatus,
            IsPublished = media.IsPublished,
            UpdatedAt = media.UpdatedAt
        };
    }
}
