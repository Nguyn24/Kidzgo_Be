using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed class StudentEnrollmentScheduleConflictService(
    IDbContext context,
    ISchedulePatternParser patternParser,
    SessionParticipantService sessionParticipantService)
{
    public const int MinimumGapMinutes = 15;
    private static readonly TimeSpan MinimumGap = TimeSpan.FromMinutes(MinimumGapMinutes);

    public async Task<Result> EnsureNoConflictsAsync(
        Guid studentProfileId,
        Guid classId,
        DateOnly assignmentStartDate,
        string? sessionSelectionPattern,
        CancellationToken cancellationToken,
        IEnumerable<StudentBookedSlot>? additionalBookedSlots = null,
        Guid? excludeEnrollmentId = null,
        Guid? excludeLegacyClassId = null,
        DateTime? excludeSlotsFromUtc = null)
    {
        var candidateSlots = await GetCandidateSlotsAsync(
            classId,
            assignmentStartDate,
            sessionSelectionPattern,
            cancellationToken);

        if (candidateSlots.Count == 0)
        {
            return Result.Success();
        }

        var fromUtc = candidateSlots.Min(s => s.Start).Add(-MinimumGap);
        var toUtc = candidateSlots.Max(s => s.End).Add(MinimumGap);

        var bookedSlots = await sessionParticipantService.GetStudentBookedSlotsDetailedAsync(
            studentProfileId,
            fromUtc,
            toUtc,
            cancellationToken);

        var relevantBookedSlots = bookedSlots
            .Where(slot => !ShouldExclude(
                slot,
                excludeEnrollmentId,
                excludeLegacyClassId,
                excludeSlotsFromUtc))
            .ToList();

        if (additionalBookedSlots is not null)
        {
            relevantBookedSlots.AddRange(additionalBookedSlots);
        }

        foreach (var candidateSlot in candidateSlots)
        {
            var conflictingSlot = relevantBookedSlots.FirstOrDefault(bookedSlot =>
                HasConflict(candidateSlot.Start, candidateSlot.End, bookedSlot.Start, bookedSlot.End));

            if (conflictingSlot != default)
            {
                return Result.Failure(EnrollmentErrors.StudentScheduleConflict(
                    conflictingSlot.ClassCode,
                    conflictingSlot.ClassTitle,
                    conflictingSlot.Start,
                    MinimumGapMinutes));
            }
        }

        return Result.Success();
    }

    public async Task<List<StudentBookedSlot>> GetCandidateSlotsAsync(
        Guid classId,
        DateOnly assignmentStartDate,
        string? sessionSelectionPattern,
        CancellationToken cancellationToken)
    {
        var sessions = await context.Sessions
            .AsNoTracking()
            .Where(s => s.ClassId == classId && s.Status != SessionStatus.Cancelled)
            .Select(s => new StudentBookedSlot(
                s.PlannedDatetime,
                s.PlannedDatetime.AddMinutes(s.DurationMinutes),
                null,
                s.ClassId,
                s.Class.Code,
                s.Class.Title,
                false))
            .ToListAsync(cancellationToken);

        return sessions
            .Where(session =>
                VietnamTime.ToVietnamDateOnly(session.Start) >= assignmentStartDate &&
                MatchesSelectionPattern(session.Start, sessionSelectionPattern))
            .OrderBy(session => session.Start)
            .ToList();
    }

    private bool MatchesSelectionPattern(DateTime sessionStartUtc, string? sessionSelectionPattern)
    {
        if (string.IsNullOrWhiteSpace(sessionSelectionPattern))
        {
            return true;
        }

        var sessionDate = VietnamTime.ToVietnamDateOnly(sessionStartUtc);
        var sessionLocalDateTime = VietnamTime.ToVietnamDateTime(sessionStartUtc);
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

    private static bool ShouldExclude(
        StudentBookedSlot slot,
        Guid? excludeEnrollmentId,
        Guid? excludeLegacyClassId,
        DateTime? excludeSlotsFromUtc)
    {
        if (excludeEnrollmentId.HasValue &&
            slot.ClassEnrollmentId == excludeEnrollmentId.Value &&
            ShouldExcludeByTime(slot.Start, excludeSlotsFromUtc))
        {
            return true;
        }

        if (excludeLegacyClassId.HasValue &&
            !slot.IsMakeup &&
            slot.ClassId == excludeLegacyClassId.Value &&
            ShouldExcludeByTime(slot.Start, excludeSlotsFromUtc))
        {
            return true;
        }

        return false;
    }

    private static bool ShouldExcludeByTime(DateTime slotStartUtc, DateTime? excludeSlotsFromUtc)
    {
        return !excludeSlotsFromUtc.HasValue || slotStartUtc >= excludeSlotsFromUtc.Value;
    }

    private static bool HasConflict(DateTime candidateStart, DateTime candidateEnd, DateTime bookedStart, DateTime bookedEnd)
    {
        var overlap = candidateStart < bookedEnd && candidateEnd > bookedStart;
        if (overlap)
        {
            return true;
        }

        var gapToStart = (candidateStart - bookedEnd).Duration();
        var gapToEnd = (bookedStart - candidateEnd).Duration();
        return gapToStart < MinimumGap || gapToEnd < MinimumGap;
    }
}
