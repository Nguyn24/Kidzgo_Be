using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports.Errors;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.SessionReports;

internal static class SessionReportScheduleGuard
{
    public static Result EnsureSessionHasEnded(Session session, DateTime nowUtc)
    {
        var sessionStartAt = session.ActualDatetime ?? session.PlannedDatetime;
        var durationMinutes = Math.Max(session.DurationMinutes, 0);
        var sessionEndAt = sessionStartAt.AddMinutes(durationMinutes);

        return nowUtc >= sessionEndAt
            ? Result.Success()
            : Result.Failure(SessionReportErrors.SessionNotEnded(sessionEndAt));
    }
}
