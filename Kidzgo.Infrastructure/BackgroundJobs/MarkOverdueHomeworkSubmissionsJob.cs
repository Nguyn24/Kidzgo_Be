using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Missions.Shared;
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
    private const string AutoGradeFeedback =
        "Automatically graded 0 because the homework was not submitted before the deadline.";

    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;
        var now = VietnamTime.UtcNow();

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IDbContext>();
        var gamificationService = scope.ServiceProvider.GetRequiredService<IGamificationService>();

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
            hs.Score = 0;
            hs.GradedAt = now;

            if (string.IsNullOrWhiteSpace(hs.TeacherFeedback))
            {
                hs.TeacherFeedback = AutoGradeFeedback;
            }
        }

        var affectedStudentIds = overdue
            .Select(hs => hs.StudentProfileId)
            .Distinct()
            .ToList();

        await db.SaveChangesAsync(cancellationToken);

        foreach (var studentProfileId in affectedStudentIds)
        {
            await HomeworkMissionProgressTracker.TrackAsync(
                db,
                gamificationService,
                studentProfileId,
                now,
                cancellationToken);
        }

        logger.LogInformation(
            "Marked {Count} overdue homework submissions as Missing and auto-graded 0",
            overdue.Count);
    }
}


