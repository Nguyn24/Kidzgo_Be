using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Homework;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Missions.Shared;

public static class HomeworkMissionProgressTracker
{
    public static async Task TrackAsync(
        IDbContext context,
        IGamificationService gamificationService,
        Guid studentProfileId,
        DateTime asOfAt,
        CancellationToken cancellationToken)
    {
        var activeMissionProgresses = await context.MissionProgresses
            .Include(mp => mp.Mission)
            .Where(mp => mp.StudentProfileId == studentProfileId)
            .Where(mp => mp.Mission.MissionType == MissionType.HomeworkStreak)
            .Where(mp => mp.Status == MissionProgressStatus.Assigned ||
                         mp.Status == MissionProgressStatus.InProgress)
            .Where(mp => mp.Mission.StartAt == null || mp.Mission.StartAt <= asOfAt)
            .Where(mp => mp.Mission.EndAt == null ||
                         mp.Mission.EndAt >= asOfAt ||
                         (mp.ProgressValue.HasValue && mp.ProgressValue.Value > 0))
            .ToListAsync(cancellationToken);

        if (activeMissionProgresses.Count == 0)
        {
            return;
        }

        var records = await GetHomeworkRecordsAsync(
            context,
            studentProfileId,
            activeMissionProgresses,
            cancellationToken);

        foreach (var missionProgress in activeMissionProgresses)
        {
            var progressValue = missionProgress.Mission.ProgressMode == MissionProgressMode.Streak
                ? CalculateHomeworkStreak(records, missionProgress.Mission, asOfAt)
                : CalculateHomeworkCount(records, missionProgress.Mission);

            await MissionProgressRewardHelper.ApplyProgressAsync(
                gamificationService,
                missionProgress,
                progressValue,
                asOfAt,
                nameof(MissionType.HomeworkStreak),
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task<List<HomeworkProgressRecord>> GetHomeworkRecordsAsync(
        IDbContext context,
        Guid studentProfileId,
        IReadOnlyCollection<MissionProgress> activeMissionProgresses,
        CancellationToken cancellationToken)
    {
        var targetClassIds = activeMissionProgresses
            .Where(mp => mp.Mission.Scope == MissionScope.Class && mp.Mission.TargetClassId.HasValue)
            .Select(mp => mp.Mission.TargetClassId!.Value)
            .Distinct()
            .ToList();

        var hasNonClassMission = activeMissionProgresses
            .Any(mp => mp.Mission.Scope != MissionScope.Class);

        var query = context.HomeworkStudents
            .AsNoTracking()
            .Where(hs => hs.StudentProfileId == studentProfileId)
            .Where(hs => hs.Assignment.DueAt.HasValue || hs.SubmittedAt.HasValue);

        if (!hasNonClassMission)
        {
            query = query.Where(hs => targetClassIds.Contains(hs.Assignment.ClassId));
        }

        return await query
            .Select(hs => new HomeworkProgressRecord(
                hs.AssignmentId,
                hs.Assignment.ClassId,
                hs.Assignment.DueAt,
                hs.SubmittedAt,
                hs.Status))
            .ToListAsync(cancellationToken);
    }

    private static int CalculateHomeworkCount(
        IEnumerable<HomeworkProgressRecord> records,
        Mission mission)
    {
        return records
            .Where(record => AppliesToMissionScope(record, mission))
            .Where(record => IsHomeworkSubmittedOnTime(record))
            .Where(record =>
            {
                var progressAt = record.DueAt ?? record.SubmittedAt!.Value;
                return IsWithinMissionWindow(progressAt, mission);
            })
            .Select(record => record.AssignmentId)
            .Distinct()
            .Count();
    }

    private static int CalculateHomeworkStreak(
        IEnumerable<HomeworkProgressRecord> records,
        Mission mission,
        DateTime asOfAt)
    {
        var eligibleRecords = records
            .Where(record => AppliesToMissionScope(record, mission))
            .Where(record => record.DueAt.HasValue)
            .Where(record => record.DueAt!.Value <= asOfAt)
            .Where(record => IsWithinMissionWindow(record.DueAt!.Value, mission))
            .OrderBy(record => VietnamTime.ToVietnamDateOnly(record.DueAt!.Value))
            .ThenBy(record => record.DueAt)
            .ThenBy(record => record.AssignmentId)
            .ToList();

        var streak = 0;
        foreach (var dueDateGroup in eligibleRecords.GroupBy(record => VietnamTime.ToVietnamDateOnly(record.DueAt!.Value)))
        {
            if (dueDateGroup.All(IsHomeworkSubmittedOnTime))
            {
                streak += dueDateGroup.Select(record => record.AssignmentId).Distinct().Count();
                continue;
            }

            streak = 0;
        }

        return streak;
    }

    private static bool AppliesToMissionScope(HomeworkProgressRecord record, Mission mission)
        => mission.Scope != MissionScope.Class ||
           !mission.TargetClassId.HasValue ||
           record.ClassId == mission.TargetClassId.Value;

    private static bool IsHomeworkSubmittedOnTime(HomeworkProgressRecord record)
        => record.SubmittedAt.HasValue &&
           (record.Status == HomeworkStatus.Submitted || record.Status == HomeworkStatus.Graded) &&
           (!record.DueAt.HasValue || record.SubmittedAt.Value <= record.DueAt.Value);

    private static bool IsWithinMissionWindow(DateTime value, Mission mission)
        => (mission.StartAt == null || value >= mission.StartAt.Value) &&
           (mission.EndAt == null || value <= mission.EndAt.Value);

    private sealed record HomeworkProgressRecord(
        Guid AssignmentId,
        Guid ClassId,
        DateTime? DueAt,
        DateTime? SubmittedAt,
        HomeworkStatus Status);
}
