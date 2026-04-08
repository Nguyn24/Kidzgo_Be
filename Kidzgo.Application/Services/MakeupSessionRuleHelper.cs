namespace Kidzgo.Application.Services;

public static class MakeupSessionRuleHelper
{
    public static DateOnly GetFirstEligibleMakeupDate(DateOnly sourceDate)
    {
        var candidate = sourceDate.AddDays(1);

        while (!IsEligibleMakeupDate(sourceDate, candidate))
        {
            candidate = candidate.AddDays(1);
        }

        return candidate;
    }

    public static bool IsEligibleMakeupDate(DateOnly sourceDate, DateOnly targetDate)
    {
        if (targetDate <= sourceDate)
        {
            return false;
        }

        if (targetDate.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
        {
            return false;
        }

        return GetWeekAnchor(targetDate) > GetWeekAnchor(sourceDate);
    }

    private static DateOnly GetWeekAnchor(DateOnly date)
    {
        return date.AddDays(DayOfWeek.Saturday - date.DayOfWeek);
    }
}
