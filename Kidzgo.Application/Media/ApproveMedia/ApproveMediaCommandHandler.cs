using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Media;
using Kidzgo.Domain.Media.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Media.ApproveMedia;

public sealed class ApproveMediaCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<ApproveMediaCommand, ApproveMediaResponse>
{
    public async Task<Result<ApproveMediaResponse>> Handle(ApproveMediaCommand command, CancellationToken cancellationToken)
    {
        var media = await context.MediaAssets
            .Include(m => m.ApprovedByUser)
            .FirstOrDefaultAsync(m => m.Id == command.Id && !m.IsDeleted, cancellationToken);

        if (media is null)
        {
            return Result.Failure<ApproveMediaResponse>(
                MediaErrors.NotFound(command.Id));
        }

        if (media.ApprovalStatus == ApprovalStatus.Approved)
        {
            return Result.Failure<ApproveMediaResponse>(
                MediaErrors.AlreadyApproved);
        }

        var approverId = userContext.UserId;
        var approver = await context.Users
            .FirstOrDefaultAsync(u => u.Id == approverId, cancellationToken);

        media.ApprovalStatus = ApprovalStatus.Approved;
        media.ApprovedById = approverId;
        media.ApprovedAt = DateTime.UtcNow;
        media.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return new ApproveMediaResponse
        {
            Id = media.Id,
            ApprovalStatus = media.ApprovalStatus,
            ApprovedById = media.ApprovedById.Value,
            ApprovedByName = approver!.Name,
            ApprovedAt = media.ApprovedAt.Value
        };
    }
}

