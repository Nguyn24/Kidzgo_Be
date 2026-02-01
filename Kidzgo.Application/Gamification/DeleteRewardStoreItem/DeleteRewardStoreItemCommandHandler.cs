using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.DeleteRewardStoreItem;

/// <summary>
/// UC-225: Xóa mềm Reward Store Item
/// </summary>
public sealed class DeleteRewardStoreItemCommandHandler(IDbContext context)
    : ICommandHandler<DeleteRewardStoreItemCommand, DeleteRewardStoreItemResponse>
{
    public async Task<Result<DeleteRewardStoreItemResponse>> Handle(
        DeleteRewardStoreItemCommand command,
        CancellationToken cancellationToken)
    {
        var item = await context.RewardStoreItems
            .FirstOrDefaultAsync(i => i.Id == command.Id && !i.IsDeleted, cancellationToken);

        if (item == null)
        {
            return Result.Failure<DeleteRewardStoreItemResponse>(
                RewardStoreErrors.NotFound(command.Id));
        }

        item.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new DeleteRewardStoreItemResponse
        {
            Id = item.Id
        });
    }
}

