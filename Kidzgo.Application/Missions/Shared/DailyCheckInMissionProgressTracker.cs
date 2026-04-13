using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Gamification;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Missions.Shared;

public static class DailyCheckInMissionProgressTracker
{
    public static async Task TrackAsync(
        IDbContext context,
        IGamificationService gamificationService,
        Guid studentProfileId,
        DateOnly checkInDate,
        CancellationToken cancellationToken)
    {
        var now = VietnamTime.UtcNow();

        var activeMissionProgresses = await context.MissionProgresses
            .Include(mp => mp.Mission)
            .Where(mp => mp.StudentProfileId == studentProfileId)
            .Where(mp => mp.Mission.MissionType == MissionType.NoUnexcusedAbsence)
            .Where(mp => mp.Status == MissionProgressStatus.Assigned ||
                         mp.Status == MissionProgressStatus.InProgress)
            .Where(mp => mp.Mission.StartAt == null || mp.Mission.StartAt <= now)
            .Where(mp => mp.Mission.EndAt == null || mp.Mission.EndAt >= now)
            .ToListAsync(cancellationToken);

        foreach (var missionProgress in activeMissionProgresses)
        {
            var progressValue = missionProgress.Mission.ProgressMode == MissionProgressMode.Streak
                ? await CalculateCheckInStreakAsync(context, studentProfileId, missionProgress.Mission, checkInDate, cancellationToken)
                : await CalculateCheckInCountAsync(context, studentProfileId, missionProgress.Mission, checkInDate, cancellationToken);

            await MissionProgressRewardHelper.ApplyProgressAsync(
                gamificationService,
                missionProgress,
                progressValue,
                now,
                nameof(MissionType.NoUnexcusedAbsence),
                cancellationToken);
        }

        if (activeMissionProgresses.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public static async Task ResetMissedStreaksAsync(
        IDbContext context,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var now = VietnamTime.UtcNow();
        var yesterday = today.AddDays(-1);

        var activeMissionProgresses = await context.MissionProgresses
            .Include(mp => mp.Mission)
            .Where(mp => mp.Mission.MissionType == MissionType.NoUnexcusedAbsence)
            .Where(mp => mp.Mission.ProgressMode == MissionProgressMode.Streak)
            .Where(mp => mp.Status == MissionProgressStatus.Assigned ||
                         mp.Status == MissionProgressStatus.InProgress)
            .Where(mp => mp.ProgressValue.HasValue && mp.ProgressValue.Value > 0)
            .Where(mp => mp.Mission.StartAt == null || mp.Mission.StartAt <= now)
            .Where(mp => mp.Mission.EndAt == null ||
                         mp.Mission.EndAt >= now ||
                         (mp.ProgressValue.HasValue && mp.ProgressValue.Value > 0))
            .ToListAsync(cancellationToken);

        foreach (var missionProgress in activeMissionProgresses)
        {
            var startDate = GetMissionStartDate(missionProgress.Mission);
            var endDate = GetMissionEndDate(missionProgress.Mission);

            var latestCheckInDate = await context.AttendanceStreaks
                .Where(s => s.StudentProfileId == missionProgress.StudentProfileId)
                .Where(s => startDate == null || s.AttendanceDate >= startDate.Value)
                .Where(s => endDate == null || s.AttendanceDate <= endDate.Value)
                .Where(s => s.AttendanceDate <= today)
                .Select(s => (DateOnly?)s.AttendanceDate)
                .MaxAsync(cancellationToken);

            if (!latestCheckInDate.HasValue || latestCheckInDate.Value < yesterday)
            {
                missionProgress.ProgressValue = 0;
                missionProgress.Status = MissionProgressStatus.Assigned;
                missionProgress.CompletedAt = null;
            }
        }

        if (activeMissionProgresses.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task<int> CalculateCheckInCountAsync(
        IDbContext context,
        Guid studentProfileId,
        Mission mission,
        DateOnly checkInDate,
        CancellationToken cancellationToken)
    {
        var startDate = GetMissionStartDate(mission);
        var endDate = GetMissionEndDate(mission);
        var effectiveEndDate = endDate.HasValue && endDate.Value < checkInDate
            ? endDate.Value
            : checkInDate;

        return await context.AttendanceStreaks
            .Where(s => s.StudentProfileId == studentProfileId)
            .Where(s => startDate == null || s.AttendanceDate >= startDate.Value)
            .Where(s => s.AttendanceDate <= effectiveEndDate)
            .Select(s => s.AttendanceDate)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    private static async Task<int> CalculateCheckInStreakAsync(
        IDbContext context,
        Guid studentProfileId,
        Mission mission,
        DateOnly checkInDate,
        CancellationToken cancellationToken)
    {
        var startDate = GetMissionStartDate(mission);
        var endDate = GetMissionEndDate(mission);
        var effectiveEndDate = endDate.HasValue && endDate.Value < checkInDate
            ? endDate.Value
            : checkInDate;

        var checkInDates = await context.AttendanceStreaks
            .Where(s => s.StudentProfileId == studentProfileId)
            .Where(s => startDate == null || s.AttendanceDate >= startDate.Value)
            .Where(s => s.AttendanceDate <= effectiveEndDate)
            .Select(s => s.AttendanceDate)
            .Distinct()
            .OrderByDescending(date => date)
            .ToListAsync(cancellationToken);

        var streak = 0;
        var expectedDate = effectiveEndDate;

        foreach (var date in checkInDates)
        {
            if (date == expectedDate)
            {
                streak++;
                expectedDate = expectedDate.AddDays(-1);
                continue;
            }

            if (date < expectedDate)
            {
                break;
            }
        }

        return streak;
    }

    private static DateOnly? GetMissionStartDate(Mission mission)
        => mission.StartAt.HasValue
            ? VietnamTime.ToVietnamDateOnly(mission.StartAt.Value)
            : null;

    private static DateOnly? GetMissionEndDate(Mission mission)
        => mission.EndAt.HasValue
            ? VietnamTime.ToVietnamDateOnly(mission.EndAt.Value)
            : null;
}
