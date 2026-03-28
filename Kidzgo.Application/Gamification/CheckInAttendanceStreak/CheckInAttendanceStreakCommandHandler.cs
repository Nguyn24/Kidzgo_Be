using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Gamification.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.CheckInAttendanceStreak;

/// <summary>
/// UC-213 to UC-220: Student tu dong diem danh hang ngay (Daily Check-in)
/// </summary>
public sealed class CheckInAttendanceStreakCommandHandler(
    IDbContext context,
    IUserContext userContext,
    IGamificationService gamificationService)
    : ICommandHandler<CheckInAttendanceStreakCommand, CheckInAttendanceStreakResponse>
{
    public async Task<Result<CheckInAttendanceStreakResponse>> Handle(
        CheckInAttendanceStreakCommand command,
        CancellationToken cancellationToken)
    {
        if (!userContext.StudentId.HasValue)
        {
            return Result.Failure<CheckInAttendanceStreakResponse>(
                StarErrors.ProfileNotFound(Guid.Empty));
        }

        var studentProfileId = userContext.StudentId.Value;

        // Verify profile exists and is Student
        var profile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == studentProfileId && p.ProfileType == ProfileType.Student, cancellationToken);

        if (profile == null)
        {
            return Result.Failure<CheckInAttendanceStreakResponse>(
                StarErrors.ProfileNotFound(studentProfileId));
        }

        // Get gamification settings (default: 1 star, 5 exp if not set)
        var settings = await context.GamificationSettings.FirstOrDefaultAsync(cancellationToken);
        var rewardStars = settings?.CheckInRewardStars ?? 1;
        var rewardExp = settings?.CheckInRewardExp ?? 5;

        // UC-213: Get today's date (UTC)
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        // UC-214: Check if already checked in today
        var existingStreak = await context.AttendanceStreaks
            .FirstOrDefaultAsync(s => s.StudentProfileId == studentProfileId && s.AttendanceDate == today, cancellationToken);

        if (existingStreak != null)
        {
            // Already checked in today, return existing record
            var allStreaksForMax = await context.AttendanceStreaks
                .Where(s => s.StudentProfileId == studentProfileId)
                .Select(s => s.CurrentStreak)
                .ToListAsync(cancellationToken);

            var existingMaxStreak = allStreaksForMax.Any() ? allStreaksForMax.Max() : 0;

            return Result.Success(new CheckInAttendanceStreakResponse
            {
                StudentProfileId = studentProfileId,
                AttendanceDate = today,
                CurrentStreak = existingStreak.CurrentStreak,
                MaxStreak = existingMaxStreak,
                RewardStars = existingStreak.RewardStars,
                RewardExp = existingStreak.RewardExp,
                IsNewStreak = false
            });
        }

        // UC-217: Get yesterday's streak
        var yesterday = today.AddDays(-1);
        var yesterdayStreak = await context.AttendanceStreaks
            .Where(s => s.StudentProfileId == studentProfileId && s.AttendanceDate == yesterday)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // Calculate current streak
        int currentStreak;
        if (yesterdayStreak != null)
        {
            // Continue streak from yesterday
            currentStreak = yesterdayStreak.CurrentStreak + 1;
        }
        else
        {
            // UC-220: Reset streak (no check-in yesterday)
            currentStreak = 1;
        }

        // UC-218: Calculate max streak
        var allStreaks = await context.AttendanceStreaks
            .Where(s => s.StudentProfileId == studentProfileId)
            .Select(s => s.CurrentStreak)
            .ToListAsync(cancellationToken);

        var maxStreak = allStreaks.Any() ? allStreaks.Max() : 0;

        if (currentStreak > maxStreak)
        {
            maxStreak = currentStreak;
        }

        // UC-214: Create Attendance Streak record
        var streak = new AttendanceStreak
        {
            Id = Guid.NewGuid(),
            StudentProfileId = studentProfileId,
            AttendanceDate = today,
            CurrentStreak = currentStreak,
            RewardStars = rewardStars,
            RewardExp = rewardExp,
            CreatedAt = DateTime.UtcNow
        };

        context.AttendanceStreaks.Add(streak);
        await context.SaveChangesAsync(cancellationToken);

        // UC-215: Add Stars
        await gamificationService.AddStarsForAttendance(
            studentProfileId,
            rewardStars,
            streak.Id,
            "Daily Check-in",
            cancellationToken);

        // UC-216: Add XP
        await gamificationService.AddXpForAttendance(
            studentProfileId,
            rewardExp,
            streak.Id,
            "Daily Check-in",
            cancellationToken);

        // ============================================================
        // GIAI DOAN 3: Track NoUnexcusedAbsence Mission progress
        // ============================================================
        var now = DateTime.UtcNow;

        var activeAttendanceMissions = await context.MissionProgresses
            .Include(mp => mp.Mission)
            .Where(mp => mp.StudentProfileId == studentProfileId)
            .Where(mp => mp.Mission.MissionType == MissionType.NoUnexcusedAbsence)
            .Where(mp => mp.Status == MissionProgressStatus.Assigned ||
                         mp.Status == MissionProgressStatus.InProgress)
            .Where(mp => mp.Mission.StartAt == null || mp.Mission.StartAt <= now)
            .Where(mp => mp.Mission.EndAt == null || mp.Mission.EndAt >= now)
            .ToListAsync(cancellationToken);

        foreach (var missionProgress in activeAttendanceMissions)
        {
            if (missionProgress.Status == MissionProgressStatus.Assigned)
            {
                missionProgress.Status = MissionProgressStatus.InProgress;
            }

            missionProgress.ProgressValue = (missionProgress.ProgressValue ?? 0) + 1;

            var totalRequired = missionProgress.Mission.TotalRequired;
            if (totalRequired.HasValue && missionProgress.ProgressValue >= totalRequired.Value)
            {
                missionProgress.Status = MissionProgressStatus.Completed;
                missionProgress.CompletedAt = now;

                if (missionProgress.Mission.RewardStars.HasValue &&
                    missionProgress.Mission.RewardStars.Value > 0)
                {
                    await gamificationService.AddStarsForMissionCompletion(
                        studentProfileId,
                        missionProgress.Mission.RewardStars.Value,
                        missionProgress.MissionId,
                        reason: $"Completed NoUnexcusedAbsence Mission: {missionProgress.Mission.Title}",
                        cancellationToken);
                }

                if (missionProgress.Mission.RewardExp.HasValue &&
                    missionProgress.Mission.RewardExp.Value > 0)
                {
                    await gamificationService.AddXpForMissionCompletion(
                        studentProfileId,
                        missionProgress.Mission.RewardExp.Value,
                        missionProgress.MissionId,
                        reason: $"Completed NoUnexcusedAbsence Mission: {missionProgress.Mission.Title}",
                        cancellationToken);
                }
            }
        }

        if (activeAttendanceMissions.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(new CheckInAttendanceStreakResponse
        {
            StudentProfileId = studentProfileId,
            AttendanceDate = today,
            CurrentStreak = currentStreak,
            MaxStreak = maxStreak,
            RewardStars = rewardStars,
            RewardExp = rewardExp,
            IsNewStreak = true
        });
    }
}
