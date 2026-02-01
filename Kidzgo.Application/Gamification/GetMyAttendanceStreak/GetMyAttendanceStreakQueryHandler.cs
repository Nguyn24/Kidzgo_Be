using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.GetMyAttendanceStreak;

public sealed class GetMyAttendanceStreakQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetMyAttendanceStreakQuery, GetMyAttendanceStreakResponse>
{
    public async Task<Result<GetMyAttendanceStreakResponse>> Handle(
        GetMyAttendanceStreakQuery query,
        CancellationToken cancellationToken)
    {
        if (!userContext.StudentId.HasValue)
        {
            return Result.Failure<GetMyAttendanceStreakResponse>(
                StarErrors.ProfileNotFound(Guid.Empty));
        }

        var studentProfileId = userContext.StudentId.Value;

        // Get all streaks for this student
        var allStreaks = await context.AttendanceStreaks
            .Where(s => s.StudentProfileId == studentProfileId)
            .OrderByDescending(s => s.AttendanceDate)
            .ToListAsync(cancellationToken);

        if (!allStreaks.Any())
        {
            return Result.Success(new GetMyAttendanceStreakResponse
            {
                StudentProfileId = studentProfileId,
                CurrentStreak = 0,
                MaxStreak = 0,
                LastAttendanceDate = null,
                RecentStreaks = new List<AttendanceStreakDto>()
            });
        }

        // UC-217: Get current streak (from most recent attendance)
        var currentStreak = allStreaks.First().CurrentStreak;

        // UC-218: Calculate max streak
        var maxStreak = allStreaks.Max(s => s.CurrentStreak);

        // Get last attendance date
        var lastAttendanceDate = allStreaks.First().AttendanceDate;

        // Get recent streaks (last 30 days)
        var recentStreaks = allStreaks
            .Take(30)
            .Select(s => new AttendanceStreakDto
            {
                Id = s.Id,
                AttendanceDate = s.AttendanceDate,
                CurrentStreak = s.CurrentStreak,
                RewardStars = s.RewardStars,
                RewardExp = s.RewardExp,
                CreatedAt = s.CreatedAt
            })
            .ToList();

        return Result.Success(new GetMyAttendanceStreakResponse
        {
            StudentProfileId = studentProfileId,
            CurrentStreak = currentStreak,
            MaxStreak = maxStreak,
            LastAttendanceDate = lastAttendanceDate,
            RecentStreaks = recentStreaks
        });
    }
}

