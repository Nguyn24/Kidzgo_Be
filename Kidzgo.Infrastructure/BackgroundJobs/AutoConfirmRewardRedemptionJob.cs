using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Gamification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Kidzgo.Infrastructure.BackgroundJobs;

/// <summary>
/// UC-235: Tự động xác nhận Reward Redemption sau N ngày (nếu không xác nhận)
/// Job chạy mỗi ngày để kiểm tra và tự động xác nhận các redemption đã được delivered >= N ngày
/// </summary>
[DisallowConcurrentExecution]
public sealed class AutoConfirmRewardRedemptionJob(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<AutoConfirmRewardRedemptionJob> logger
) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;
        logger.LogInformation("AutoConfirmRewardRedemptionJob started at {Time}", DateTime.UtcNow);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDbContext>();

            // Đọc số ngày từ config, fallback là 3 nếu không có
            var daysToWait = configuration.GetValue<int>("Quartz:Schedules:AutoConfirmRewardRedemptionJob_Days", 3);
            
            // Tính toán ngày N ngày trước (UTC)
            var daysAgo = DateTime.UtcNow.Date.AddDays(-daysToWait);

            // Tìm các Reward Redemption cần tự động xác nhận:
            // - Status = Delivered
            // - DeliveredAt <= N ngày trước (từ config)
            // - ReceivedAt = null (chưa được xác nhận)
            var redemptionsToAutoConfirm = await db.RewardRedemptions
                .Where(r => r.Status == RedemptionStatus.Delivered
                            && r.DeliveredAt.HasValue
                            && r.DeliveredAt.Value.Date <= daysAgo
                            && r.ReceivedAt == null)
                .ToListAsync(cancellationToken);
            
            logger.LogInformation("Checking redemptions delivered >= {Days} days ago (before {Date})", daysToWait, daysAgo);

            if (!redemptionsToAutoConfirm.Any())
            {
                logger.LogInformation("No reward redemptions to auto-confirm");
                return;
            }

            var now = DateTime.UtcNow;
            var confirmedCount = 0;

            foreach (var redemption in redemptionsToAutoConfirm)
            {
                redemption.Status = RedemptionStatus.Received;
                redemption.ReceivedAt = now;
                confirmedCount++;

                logger.LogInformation(
                    "Auto-confirmed Reward Redemption {RedemptionId} for Student {StudentProfileId}, Item: {ItemName}",
                    redemption.Id, redemption.StudentProfileId, redemption.ItemName);
            }

            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "AutoConfirmRewardRedemptionJob completed. Auto-confirmed {Count} reward redemptions",
                confirmedCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred in AutoConfirmRewardRedemptionJob");
            throw; // Re-throw to let Quartz handle retry
        }
    }
}

