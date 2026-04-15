using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Kidzgo.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public sealed class ResumePausedEnrollmentsJob(
    IServiceScopeFactory scopeFactory,
    ILogger<ResumePausedEnrollmentsJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;
        var today = VietnamTime.ToVietnamDateOnly(VietnamTime.UtcNow());

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IDbContext>();
        var reactivationService = scope.ServiceProvider.GetRequiredService<PauseEnrollmentReactivationService>();

        var duePauseRequestIds = await db.PauseEnrollmentRequests
            .AsNoTracking()
            .Where(request => request.Status == PauseEnrollmentRequestStatus.Approved
                && (request.Outcome == PauseEnrollmentOutcome.ContinueSameClass ||
                    request.Outcome == PauseEnrollmentOutcome.ContinueWithTutoring)
                && request.OutcomeAt.HasValue
                && request.PauseTo < today
                && !db.PauseEnrollmentRequestHistories.Any(history =>
                    history.PauseEnrollmentRequestId == request.Id &&
                    history.NewStatus == EnrollmentStatus.Active))
            .Select(request => request.Id)
            .ToListAsync(cancellationToken);

        if (duePauseRequestIds.Count == 0)
        {
            return;
        }

        var reactivatedCount = 0;

        foreach (var pauseRequestId in duePauseRequestIds)
        {
            var result = await reactivationService.ReactivateIfDueAsync(
                pauseRequestId,
                changedBy: null,
                cancellationToken);

            if (result.IsFailure)
            {
                logger.LogWarning(
                    "ResumePausedEnrollmentsJob skipped pause request {PauseRequestId}: {ErrorCode} - {ErrorDescription}",
                    pauseRequestId,
                    result.Error.Code,
                    result.Error.Description);
                continue;
            }

            if (result.Value > 0)
            {
                reactivatedCount += result.Value;
                logger.LogInformation(
                    "ResumePausedEnrollmentsJob reactivated {EnrollmentCount} enrollment(s) for pause request {PauseRequestId}",
                    result.Value,
                    pauseRequestId);
            }
        }

        logger.LogInformation(
            "ResumePausedEnrollmentsJob processed {PauseRequestCount} pause request(s), reactivated {EnrollmentCount} enrollment(s)",
            duePauseRequestIds.Count,
            reactivatedCount);
    }
}
