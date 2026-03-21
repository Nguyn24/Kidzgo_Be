using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Kidzgo.Infrastructure.BackgroundJobs;

/// <summary>
/// UC-132: Đánh dấu Homework quá hạn (LATE/MISSING)
/// Job quét các HomeworkStudent quá hạn nhưng chưa nộp để chuyển từ Assigned -> Missing.
/// </summary>
[DisallowConcurrentExecution]
public sealed class MarkOverdueHomeworkSubmissionsJob(
    IServiceScopeFactory scopeFactory,
    ILogger<MarkOverdueHomeworkSubmissionsJob> logger
) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;
        var now = DateTime.UtcNow;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IDbContext>();

        // Overdue = due date passed, not submitted yet, and still Assigned
        var overdue = await db.HomeworkStudents
            .Include(hs => hs.Assignment)
            .Where(hs =>
                hs.Status == HomeworkStatus.Assigned &&
                hs.SubmittedAt == null &&
                hs.Assignment.DueAt.HasValue &&
                hs.Assignment.DueAt.Value < now)
            .ToListAsync(cancellationToken);

        if (overdue.Count == 0)
        {
            return;
        }

        foreach (var hs in overdue)
        {
            hs.Status = HomeworkStatus.Missing;
            
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Marked {Count} overdue homework submissions as Missing", overdue.Count);
    }
}


