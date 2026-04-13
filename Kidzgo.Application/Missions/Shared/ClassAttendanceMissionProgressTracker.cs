using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Missions.Shared;

public static class ClassAttendanceMissionProgressTracker
{
    public static async Task TrackAsync(
        IDbContext context,
        IGamificationService gamificationService,
        Guid studentProfileId,
        Session session,
        CancellationToken cancellationToken)
    {
        var attendanceAt = session.ActualDatetime ?? session.PlannedDatetime;
        var now = VietnamTime.UtcNow();

        var activeMissionProgresses = await context.MissionProgresses
            .Include(mp => mp.Mission)
            .Where(mp => mp.StudentProfileId == studentProfileId)
            .Where(mp => mp.Mission.MissionType == MissionType.ClassAttendance)
            .Where(mp => mp.Status == MissionProgressStatus.Assigned ||
                         mp.Status == MissionProgressStatus.InProgress)
            .Where(mp => mp.Mission.Scope != MissionScope.Class ||
                         mp.Mission.TargetClassId == session.ClassId)
            .Where(mp => mp.Mission.CreatedAt <= attendanceAt)
            .Where(mp => mp.Mission.StartAt == null || mp.Mission.StartAt <= attendanceAt)
            .Where(mp => mp.Mission.EndAt == null || mp.Mission.EndAt >= attendanceAt)
            .ToListAsync(cancellationToken);

        foreach (var missionProgress in activeMissionProgresses)
        {
            var asOfAt = missionProgress.Mission.EndAt.HasValue && missionProgress.Mission.EndAt.Value < now
                ? missionProgress.Mission.EndAt.Value
                : now;

            var progressValue = missionProgress.Mission.ProgressMode == MissionProgressMode.Streak
                ? await CalculateClassAttendanceStreakAsync(context, studentProfileId, missionProgress.Mission, asOfAt, cancellationToken)
                : await CalculateClassAttendanceCountAsync(context, studentProfileId, missionProgress.Mission, asOfAt, cancellationToken);

            await MissionProgressRewardHelper.ApplyProgressAsync(
                gamificationService,
                missionProgress,
                progressValue,
                now,
                nameof(MissionType.ClassAttendance),
                cancellationToken);
        }

        if (activeMissionProgresses.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task<int> CalculateClassAttendanceCountAsync(
        IDbContext context,
        Guid studentProfileId,
        Mission mission,
        DateTime asOfAt,
        CancellationToken cancellationToken)
    {
        var targetClassId = mission.Scope == MissionScope.Class
            ? mission.TargetClassId
            : null;

        return await context.Attendances
            .Where(a => a.StudentProfileId == studentProfileId)
            .Where(a => a.AttendanceStatus == AttendanceStatus.Present)
            .Where(a => !targetClassId.HasValue || a.Session.ClassId == targetClassId.Value)
            .Where(a => (a.Session.ActualDatetime ?? a.Session.PlannedDatetime) >= GetMissionEffectiveStartAt(mission))
            .Where(a => mission.EndAt == null || (a.Session.ActualDatetime ?? a.Session.PlannedDatetime) <= mission.EndAt)
            .Where(a => (a.Session.ActualDatetime ?? a.Session.PlannedDatetime) <= asOfAt)
            .Select(a => a.SessionId)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    private static async Task<int> CalculateClassAttendanceStreakAsync(
        IDbContext context,
        Guid studentProfileId,
        Mission mission,
        DateTime asOfAt,
        CancellationToken cancellationToken)
    {
        var targetClassId = mission.Scope == MissionScope.Class
            ? mission.TargetClassId
            : null;

        var records = await context.Attendances
            .Where(a => a.StudentProfileId == studentProfileId)
            .Where(a => !targetClassId.HasValue || a.Session.ClassId == targetClassId.Value)
            .Where(a => (a.Session.ActualDatetime ?? a.Session.PlannedDatetime) >= GetMissionEffectiveStartAt(mission))
            .Where(a => mission.EndAt == null || (a.Session.ActualDatetime ?? a.Session.PlannedDatetime) <= mission.EndAt)
            .Where(a => (a.Session.ActualDatetime ?? a.Session.PlannedDatetime) <= asOfAt)
            .OrderBy(a => a.Session.ActualDatetime ?? a.Session.PlannedDatetime)
            .ThenBy(a => a.SessionId)
            .Select(a => new AttendanceProgressRecord(
                a.SessionId,
                a.AttendanceStatus))
            .ToListAsync(cancellationToken);

        var streak = 0;
        foreach (var record in records)
        {
            if (record.AttendanceStatus == AttendanceStatus.Present)
            {
                streak++;
                continue;
            }

            if (record.AttendanceStatus == AttendanceStatus.Absent)
            {
                streak = 0;
            }
        }

        return streak;
    }

    private static DateTime GetMissionEffectiveStartAt(Mission mission)
        => mission.StartAt.HasValue && mission.StartAt.Value > mission.CreatedAt
            ? mission.StartAt.Value
            : mission.CreatedAt;

    private sealed record AttendanceProgressRecord(Guid SessionId, AttendanceStatus AttendanceStatus);
}
