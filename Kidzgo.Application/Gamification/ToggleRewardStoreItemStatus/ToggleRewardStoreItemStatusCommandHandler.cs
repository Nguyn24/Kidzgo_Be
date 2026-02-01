using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.ToggleRewardStoreItemStatus;

public sealed class ToggleRewardStoreItemStatusCommandHandler(
    IDbContext context
) : ICommandHandler<ToggleRewardStoreItemStatusCommand, ToggleRewardStoreItemStatusResponse>
{
    public async Task<Result<ToggleRewardStoreItemStatusResponse>> Handle(
        ToggleRewardStoreItemStatusCommand command,
        CancellationToken cancellationToken)
    {
        var item = await context.RewardStoreItems
            .FirstOrDefaultAsync(item => item.Id == command.Id && !item.IsDeleted, cancellationToken);

        if (item is null)
        {
            return Result.Failure<ToggleRewardStoreItemStatusResponse>(
                RewardStoreErrors.NotFound(command.Id));
        }

        item.IsActive = !item.IsActive;
        item.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new ToggleRewardStoreItemStatusResponse
        {
            Id = item.Id,
            IsActive = item.IsActive
        });
    }
}

