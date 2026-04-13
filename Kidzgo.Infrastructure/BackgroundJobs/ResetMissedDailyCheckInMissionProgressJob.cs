using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Missions.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Kidzgo.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public sealed class ResetMissedDailyCheckInMissionProgressJob(
    IServiceScopeFactory scopeFactory,
    ILogger<ResetMissedDailyCheckInMissionProgressJob> logger
) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;
        var today = VietnamTime.TodayDateOnly();

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IDbContext>();

        await DailyCheckInMissionProgressTracker.ResetMissedStreaksAsync(
            db,
            today,
            cancellationToken);

        logger.LogInformation(
            "Reset missed daily check-in streak mission progress before {Today}",
            today);
    }
}
