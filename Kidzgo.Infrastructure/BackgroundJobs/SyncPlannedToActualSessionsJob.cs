using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Kidzgo.Infrastructure.BackgroundJobs;

public sealed class SyncPlannedToActualSessionsJob(
    IServiceScopeFactory scopeFactory,
    ILogger<SyncPlannedToActualSessionsJob> logger
) : IJob
{
    private static readonly TimeSpan LookbackWindow = TimeSpan.FromDays(1);

    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IDbContext>();

        var now = DateTime.UtcNow;
        var from = now.Subtract(LookbackWindow);

        var sessions = await db.Sessions
            .Where(s =>
                s.Status == SessionStatus.Scheduled &&
                !s.ActualDatetime.HasValue &&
                s.PlannedDatetime <= now &&
                s.PlannedDatetime >= from)
            .ToListAsync(cancellationToken);

        if (sessions.Count == 0)
        {
            return;
        }

        foreach (var s in sessions)
        {
            s.ActualDatetime ??= s.PlannedDatetime;
            s.ActualRoomId ??= s.PlannedRoomId;
            s.ActualTeacherId ??= s.PlannedTeacherId;
            s.ActualAssistantId ??= s.PlannedAssistantId;
            s.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Quartz job synced Planned* -> Actual* for {Count} sessions", sessions.Count);
    }
}


