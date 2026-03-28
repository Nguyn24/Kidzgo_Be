using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.GetGamificationSettings;

public sealed class GetGamificationSettingsQueryHandler(
    IDbContext context
) : IQueryHandler<GetGamificationSettingsQuery, GetGamificationSettingsResponse>
{
    public async Task<Result<GetGamificationSettingsResponse>> Handle(
        GetGamificationSettingsQuery query,
        CancellationToken cancellationToken)
    {
        var settings = await context.GamificationSettings.FirstOrDefaultAsync(cancellationToken);

        return Result.Success(new GetGamificationSettingsResponse
        {
            CheckInRewardStars = settings?.CheckInRewardStars ?? 1,
            CheckInRewardExp = settings?.CheckInRewardExp ?? 5
        });
    }
}
