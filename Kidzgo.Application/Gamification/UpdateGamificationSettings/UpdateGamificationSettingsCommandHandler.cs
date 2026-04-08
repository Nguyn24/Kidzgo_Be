using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.UpdateGamificationSettings;

public sealed class UpdateGamificationSettingsCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateGamificationSettingsCommand, UpdateGamificationSettingsResponse>
{
    public async Task<Result<UpdateGamificationSettingsResponse>> Handle(
        UpdateGamificationSettingsCommand command,
        CancellationToken cancellationToken)
    {
        var settings = await context.GamificationSettings.FirstOrDefaultAsync(cancellationToken);

        if (settings is null)
        {
            // Create new if not exists
            settings = new Domain.Gamification.GamificationSettings
            {
                Id = 1,
                CheckInRewardStars = command.CheckInRewardStars,
                CheckInRewardExp = command.CheckInRewardExp,
                CreatedAt = VietnamTime.UtcNow()
            };
            context.GamificationSettings.Add(settings);
        }
        else
        {
            settings.CheckInRewardStars = command.CheckInRewardStars;
            settings.CheckInRewardExp = command.CheckInRewardExp;
            settings.UpdatedAt = VietnamTime.UtcNow();
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateGamificationSettingsResponse
        {
            CheckInRewardStars = settings.CheckInRewardStars,
            CheckInRewardExp = settings.CheckInRewardExp
        });
    }
}
