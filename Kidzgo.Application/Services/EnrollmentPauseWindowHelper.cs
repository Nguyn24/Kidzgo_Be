using Kidzgo.Domain.Classes;

namespace Kidzgo.Application.Services;

internal readonly record struct EnrollmentPauseWindow(
    Guid EnrollmentId,
    DateOnly PauseFrom,
    DateOnly PauseTo);

internal static class EnrollmentPauseWindowHelper
{
    public static Dictionary<Guid, List<EnrollmentPauseWindow>> BuildLookup(
        IEnumerable<EnrollmentPauseWindow> pauseWindows)
    {
        return pauseWindows
            .GroupBy(window => window.EnrollmentId)
            .ToDictionary(group => group.Key, group => group.ToList());
    }

    public static bool IsPausedOn(
        Guid enrollmentId,
        DateOnly sessionDate,
        IReadOnlyDictionary<Guid, List<EnrollmentPauseWindow>> pauseLookup)
    {
        return pauseLookup.TryGetValue(enrollmentId, out var pauseWindows)
            && pauseWindows.Any(window =>
                window.PauseFrom <= sessionDate &&
                window.PauseTo >= sessionDate);
    }
}
