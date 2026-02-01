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
/// UC-213 to UC-220: Student tự điểm danh hàng ngày (Daily Check-in)
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

