using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Gamification.Errors;
using Kidzgo.Domain.Users;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.CheckInAttendanceStreak;

/// <summary>
/// UC-213 to UC-220: Student tu dong diem danh hang ngay (Daily Check-in)
/// </summary>
public sealed class CheckInAttendanceStreakCommandHandler(
    IDbContext context,
    IUserContext userContext,
    IGamificationService gamificationService,
    ILogger<CheckInAttendanceStreakCommandHandler> logger)
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
            RewardStars = 1, // UC-215: 1 star per check-in
            RewardExp = 5,  // UC-216: 5 exp per check-in
            CreatedAt = DateTime.UtcNow
        };

        context.AttendanceStreaks.Add(streak);
        await context.SaveChangesAsync(cancellationToken);

        // UC-215: Add Stars (1 star)
        await gamificationService.AddStarsForAttendance(
            studentProfileId,
            1,
            streak.Id, // Use streak ID as sourceId
            "Daily Check-in",
            cancellationToken);

        // UC-216: Add XP (5 exp)
        await gamificationService.AddXpForAttendance(
            studentProfileId,
            5,
            streak.Id, // Use streak ID as sourceId
            "Daily Check-in",
            cancellationToken);

        // ============================================================
        // GIAI DOAN 3: Track NoUnexcusedAbsence Mission progress
        // Tang ProgressValue len 1 khi student check-in thanh cong
        // ============================================================
        var now = DateTime.UtcNow;

        logger.LogInformation(
            "[MissionDebug] Student={StudentId} checking NoUnexcusedAbsence missions at {Now}",
            studentProfileId, now);

        // Tim tat ca NoUnexcusedAbsence missions dang active cua student
        // Query nao dang bi loi? Kiem tra tung where clause
        var allStudentProgress = await context.MissionProgresses
            .Include(mp => mp.Mission)
            .Where(mp => mp.StudentProfileId == studentProfileId)
            .Where(mp => mp.Mission.MissionType == MissionType.NoUnexcusedAbsence)
            .ToListAsync(cancellationToken);

        logger.LogInformation(
            "[MissionDebug] Found {Count} NoUnexcusedAbsence progress records for student",
            allStudentProgress.Count);

        foreach (var p in allStudentProgress)
        {
            logger.LogInformation(
                "[MissionDebug] Progress: MissionId={MissionId}, Status={Status}, ProgressValue={ProgressValue}, " +
                "TotalRequired={TotalRequired}, StartAt={StartAt}, EndAt={EndAt}, Now={Now}, " +
                "StartBeforeNow={StartBefore}, EndAfterNow={EndAfter}",
                p.MissionId, p.Status, p.ProgressValue,
                p.Mission.TotalRequired, p.Mission.StartAt, p.Mission.EndAt, now,
                p.Mission.StartAt == null || p.Mission.StartAt <= now,
                p.Mission.EndAt == null || p.Mission.EndAt >= now);
        }

        // Filter them dang active (trong khoang thoi gian)
        var activeAttendanceMissions = allStudentProgress
            .Where(mp => mp.Status == MissionProgressStatus.Assigned ||
                         mp.Status == MissionProgressStatus.InProgress)
            .Where(mp => mp.Mission.StartAt == null || mp.Mission.StartAt <= now)
            .Where(mp => mp.Mission.EndAt == null || mp.Mission.EndAt >= now)
            .ToList();

        logger.LogInformation(
            "[MissionDebug] {ActiveCount} missions are active (Assigned/InProgress + in date range)",
            activeAttendanceMissions.Count);

        foreach (var missionProgress in activeAttendanceMissions)
        {
            // Update status to InProgress if currently Assigned
            if (missionProgress.Status == MissionProgressStatus.Assigned)
            {
                missionProgress.Status = MissionProgressStatus.InProgress;
            }

            // Increment progress value
            missionProgress.ProgressValue = (missionProgress.ProgressValue ?? 0) + 1;

            logger.LogInformation(
                "[MissionDebug] Incremented MissionId={MissionId}, new ProgressValue={ProgressValue}",
                missionProgress.MissionId, missionProgress.ProgressValue);

            // Check if mission is completed (reached TotalRequired)
            var totalRequired = missionProgress.Mission.TotalRequired;
            if (totalRequired.HasValue && missionProgress.ProgressValue >= totalRequired.Value)
            {
                missionProgress.Status = MissionProgressStatus.Completed;
                missionProgress.CompletedAt = now;

                logger.LogInformation(
                    "[MissionDebug] MissionId={MissionId} COMPLETED! RewardStars={Stars}, RewardExp={Exp}",
                    missionProgress.MissionId,
                    missionProgress.Mission.RewardStars,
                    missionProgress.Mission.RewardExp);

                // Cong Mission Reward Stars
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

                // Cong Mission Reward XP
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

        // Save mission progress changes
        if (activeAttendanceMissions.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("[MissionDebug] Saved {Count} mission progress changes", activeAttendanceMissions.Count);
        }

        return Result.Success(new CheckInAttendanceStreakResponse
        {
            StudentProfileId = studentProfileId,
            AttendanceDate = today,
            CurrentStreak = currentStreak,
            MaxStreak = maxStreak,
            RewardStars = 1,
            RewardExp = 5,
            IsNewStreak = true
        });
    }
}
