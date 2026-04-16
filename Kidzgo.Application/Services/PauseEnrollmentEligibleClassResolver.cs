using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed class PauseEnrollmentEligibleClassResolver(
    IDbContext context,
    ISchedulePatternParser patternParser)
{
    public async Task<List<Guid>> GetEligibleClassIdsAsync(
        Guid studentProfileId,
        DateOnly pauseFrom,
        DateOnly pauseTo,
        CancellationToken cancellationToken)
    {
        var activeEnrollments = await context.ClassEnrollments
            .AsNoTracking()
            .Where(e => e.StudentProfileId == studentProfileId && e.Status == EnrollmentStatus.Active)
            .Select(e => new ActiveEnrollmentSnapshot(
                e.Id,
                e.ClassId,
                e.EnrollDate,
                e.SessionSelectionPattern))
            .ToListAsync(cancellationToken);

        if (activeEnrollments.Count == 0)
        {
            return [];
        }

        var pauseFromUtc = VietnamTime.TreatAsVietnamLocal(pauseFrom.ToDateTime(TimeOnly.MinValue));
        var pauseToUtc = VietnamTime.EndOfVietnamDayUtc(
            VietnamTime.TreatAsVietnamLocal(pauseTo.ToDateTime(TimeOnly.MinValue)));

        var activeEnrollmentIds = activeEnrollments
            .Select(e => e.Id)
            .ToList();

        var assignedClassIds = await context.StudentSessionAssignments
            .AsNoTracking()
            .Where(a => a.StudentProfileId == studentProfileId
                && a.Status == StudentSessionAssignmentStatus.Assigned
                && activeEnrollmentIds.Contains(a.ClassEnrollmentId)
                && a.Session.Status != SessionStatus.Cancelled
                && a.Session.PlannedDatetime >= pauseFromUtc
                && a.Session.PlannedDatetime <= pauseToUtc)
            .Select(a => a.Session.ClassId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var activeClassIds = activeEnrollments
            .Select(e => e.ClassId)
            .Distinct()
            .ToList();

        var pauseLookup = EnrollmentPauseWindowHelper.BuildLookup(
            await context.PauseEnrollmentRequestHistories
                .AsNoTracking()
                .Where(history => history.EnrollmentId.HasValue
                    && activeEnrollmentIds.Contains(history.EnrollmentId.Value)
                    && history.NewStatus == EnrollmentStatus.Paused)
                .Select(history => new EnrollmentPauseWindow(
                    history.EnrollmentId!.Value,
                    history.PauseFrom,
                    history.PauseTo))
                .ToListAsync(cancellationToken));

        var legacySessions = await context.Sessions
            .AsNoTracking()
            .Where(s => activeClassIds.Contains(s.ClassId)
                && s.Status != SessionStatus.Cancelled
                && !context.StudentSessionAssignments.Any(a => a.SessionId == s.Id)
                && s.PlannedDatetime >= pauseFromUtc
                && s.PlannedDatetime <= pauseToUtc)
            .Select(s => new LegacySessionSnapshot(
                s.ClassId,
                s.PlannedDatetime))
            .ToListAsync(cancellationToken);

        var legacyClassIds = legacySessions
            .SelectMany(session => activeEnrollments
                .Where(enrollment =>
                    enrollment.ClassId == session.ClassId &&
                    enrollment.EnrollDate <= VietnamTime.ToVietnamDateOnly(session.PlannedDatetime) &&
                    !EnrollmentPauseWindowHelper.IsPausedOn(
                        enrollment.Id,
                        VietnamTime.ToVietnamDateOnly(session.PlannedDatetime),
                        pauseLookup) &&
                    MatchesSelectionPattern(session.PlannedDatetime, enrollment.SessionSelectionPattern))
                .Select(enrollment => enrollment.ClassId))
            .Distinct()
            .ToList();

        return assignedClassIds
            .Union(legacyClassIds)
            .ToList();
    }

    private bool MatchesSelectionPattern(DateTime sessionPlannedDatetime, string? sessionSelectionPattern)
    {
        if (string.IsNullOrWhiteSpace(sessionSelectionPattern))
        {
            return true;
        }

        var sessionDate = VietnamTime.ToVietnamDateOnly(sessionPlannedDatetime);
        var sessionLocalDateTime = VietnamTime.ToVietnamDateTime(sessionPlannedDatetime);
        var parseResult = patternParser.ParseAndGenerateOccurrences(
            sessionSelectionPattern,
            sessionDate,
            sessionDate);

        if (parseResult.IsFailure)
        {
            return false;
        }

        return parseResult.Value.Any(occurrence =>
            Math.Abs((occurrence - sessionLocalDateTime).TotalMinutes) < 1);
    }

    private sealed record ActiveEnrollmentSnapshot(
        Guid Id,
        Guid ClassId,
        DateOnly EnrollDate,
        string? SessionSelectionPattern);

    private sealed record LegacySessionSnapshot(
        Guid ClassId,
        DateTime PlannedDatetime);
}
