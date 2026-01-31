using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.GetAttendanceStreak;

public sealed class GetAttendanceStreakQueryHandler(
    IDbContext context
) : IQueryHandler<GetAttendanceStreakQuery, GetAttendanceStreakResponse>
{
    public async Task<Result<GetAttendanceStreakResponse>> Handle(
        GetAttendanceStreakQuery query,
        CancellationToken cancellationToken)
    {
        // Get all streaks for this student
        var allStreaks = await context.AttendanceStreaks
            .Where(s => s.StudentProfileId == query.StudentProfileId)
            .OrderByDescending(s => s.AttendanceDate)
            .ToListAsync(cancellationToken);

        if (!allStreaks.Any())
        {
            return Result.Success(new GetAttendanceStreakResponse
            {
                StudentProfileId = query.StudentProfileId,
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

        return Result.Success(new GetAttendanceStreakResponse
        {
            StudentProfileId = query.StudentProfileId,
            CurrentStreak = currentStreak,
            MaxStreak = maxStreak,
            LastAttendanceDate = lastAttendanceDate,
            RecentStreaks = recentStreaks
        });
    }
}

