using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.ToggleMissionRewardRuleStatus;

public sealed class ToggleMissionRewardRuleStatusCommandHandler(IDbContext context)
    : ICommandHandler<ToggleMissionRewardRuleStatusCommand, ToggleMissionRewardRuleStatusResponse>
{
    public async Task<Result<ToggleMissionRewardRuleStatusResponse>> Handle(
        ToggleMissionRewardRuleStatusCommand command,
        CancellationToken cancellationToken)
    {
        var rule = await context.MissionRewardRules
            .FirstOrDefaultAsync(rule => rule.Id == command.Id, cancellationToken);

        if (rule is null)
        {
            return Result.Failure<ToggleMissionRewardRuleStatusResponse>(
                MissionRewardRuleErrors.NotFound(command.Id));
        }

        rule.IsActive = !rule.IsActive;
        rule.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new ToggleMissionRewardRuleStatusResponse
        {
            Id = rule.Id,
            IsActive = rule.IsActive
        });
    }
}
