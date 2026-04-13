using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Missions.Shared;
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
        var today = VietnamTime.TodayDateOnly();

        // UC-214: Check if already checked in today
        var existingStreak = await context.AttendanceStreaks
            .FirstOrDefaultAsync(s => s.StudentProfileId == studentProfileId && s.AttendanceDate == today, cancellationToken);

        if (existingStreak != null)
        {
            await DailyCheckInMissionProgressTracker.TrackAsync(
                context,
                gamificationService,
                studentProfileId,
                today,
                cancellationToken);

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
            CreatedAt = VietnamTime.UtcNow()
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

        await DailyCheckInMissionProgressTracker.TrackAsync(
            context,
            gamificationService,
            studentProfileId,
            today,
            cancellationToken);

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
