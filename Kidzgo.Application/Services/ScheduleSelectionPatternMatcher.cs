using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.Application.Services;

internal static class ScheduleSelectionPatternMatcher
{
    public static bool Matches(
        DateTime sessionPlannedDatetime,
        string? sessionSelectionPattern,
        ISchedulePatternParser patternParser)
    {
        if (string.IsNullOrWhiteSpace(sessionSelectionPattern))
        {
            return true;
        }

        foreach (var sessionWallClock in GetSessionWallClockCandidates(sessionPlannedDatetime))
        {
            var sessionDate = DateOnly.FromDateTime(sessionWallClock);
            var parseResult = patternParser.ParseAndGenerateOccurrences(
                sessionSelectionPattern,
                sessionDate,
                sessionDate);

            if (parseResult.IsFailure)
            {
                continue;
            }

            if (parseResult.Value.Any(occurrence =>
                    IsSameMinute(ToVietnamWallClock(occurrence), sessionWallClock)))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<DateTime> GetSessionWallClockCandidates(DateTime sessionPlannedDatetime)
    {
        var vietnamWallClock = VietnamTime.ToVietnamDateTime(sessionPlannedDatetime);
        yield return vietnamWallClock;

        var storedWallClock = DateTime.SpecifyKind(sessionPlannedDatetime, DateTimeKind.Unspecified);
        if (!IsSameMinute(storedWallClock, vietnamWallClock))
        {
            yield return storedWallClock;
        }
    }

    private static DateTime ToVietnamWallClock(DateTime value)
    {
        return VietnamTime.ToVietnamDateTime(value);
    }

    private static bool IsSameMinute(DateTime left, DateTime right)
    {
        return Math.Abs((left - right).TotalMinutes) < 1;
    }
}
