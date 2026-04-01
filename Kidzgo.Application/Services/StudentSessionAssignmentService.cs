using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public readonly record struct RegularSessionParticipant(
    Guid StudentProfileId,
    Guid? ClassEnrollmentId,
    Guid? RegistrationId,
    RegistrationTrackType Track);

public sealed class StudentSessionAssignmentService(
    IDbContext context,
    ISchedulePatternParser patternParser)
{
    public Result ValidateSelectionPattern(Class classEntity, string? sessionSelectionPattern)
    {
        if (string.IsNullOrWhiteSpace(sessionSelectionPattern))
        {
            return Result.Success();
        }

        var validationEndDate = classEntity.EndDate ?? classEntity.StartDate.AddMonths(3);
        var selectionResult = patternParser.ParseAndGenerateOccurrences(
            sessionSelectionPattern,
            classEntity.StartDate,
            validationEndDate);

        if (selectionResult.IsFailure)
        {
            return Result.Failure(Error.Validation(
                "Enrollment.SessionSelectionPatternInvalid",
                selectionResult.Error.Description));
        }

        if (selectionResult.Value.Count == 0)
        {
            return Result.Failure(Error.Validation(
                "Enrollment.SessionSelectionPatternEmpty",
                "Session selection pattern does not match any session slot in the validation range."));
        }

        if (string.IsNullOrWhiteSpace(classEntity.SchedulePattern))
        {
            return Result.Success();
        }

        var classResult = patternParser.ParseAndGenerateOccurrences(
            classEntity.SchedulePattern,
            classEntity.StartDate,
            validationEndDate);

        if (classResult.IsFailure)
        {
            return Result.Failure(Error.Validation(
                "Enrollment.ClassSchedulePatternInvalid",
                classResult.Error.Description));
        }

        var classOccurrences = classResult.Value
            .Select(ToMinuteKey)
            .ToHashSet();

        var isSubset = selectionResult.Value
            .Select(ToMinuteKey)
            .All(classOccurrences.Contains);

        if (!isSubset)
        {
            return Result.Failure(Error.Validation(
                "Enrollment.SessionSelectionPatternMismatch",
                "Session selection pattern must be a subset of the class schedule pattern."));
        }

        return Result.Success();
    }

    public async Task SyncAssignmentsForEnrollmentAsync(
        ClassEnrollment enrollment,
        CancellationToken cancellationToken)
    {
        var sessionDateFrom = enrollment.EnrollDate;
        var sessions = await context.Sessions
            .Where(s => s.ClassId == enrollment.ClassId)
            .ToListAsync(cancellationToken);

        var existingAssignments = await context.StudentSessionAssignments
            .Where(a => a.ClassEnrollmentId == enrollment.Id)
            .ToListAsync(cancellationToken);

        var assignmentsBySessionId = existingAssignments.ToDictionary(a => a.SessionId);
        var now = DateTime.UtcNow;

        foreach (var session in sessions)
        {
            var shouldBeAssigned =
                session.Status != SessionStatus.Cancelled &&
                DateOnly.FromDateTime(session.PlannedDatetime) >= sessionDateFrom &&
                MatchesSelectionPattern(session, enrollment.SessionSelectionPattern);

            assignmentsBySessionId.TryGetValue(session.Id, out var assignment);

            if (shouldBeAssigned)
            {
                if (assignment == null)
                {
                    context.StudentSessionAssignments.Add(new StudentSessionAssignment
                    {
                        Id = Guid.NewGuid(),
                        SessionId = session.Id,
                        StudentProfileId = enrollment.StudentProfileId,
                        ClassEnrollmentId = enrollment.Id,
                        RegistrationId = enrollment.RegistrationId,
                        Track = enrollment.Track,
                        Status = StudentSessionAssignmentStatus.Assigned,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }
                else
                {
                    assignment.StudentProfileId = enrollment.StudentProfileId;
                    assignment.RegistrationId = enrollment.RegistrationId;
                    assignment.Track = enrollment.Track;
                    assignment.Status = StudentSessionAssignmentStatus.Assigned;
                    assignment.UpdatedAt = now;
                }
            }
            else if (assignment is not null && assignment.Status != StudentSessionAssignmentStatus.Cancelled)
            {
                assignment.Status = StudentSessionAssignmentStatus.Cancelled;
                assignment.UpdatedAt = now;
            }
        }
    }

    public async Task SyncAssignmentsForSessionAsync(
        Session session,
        CancellationToken cancellationToken)
    {
        var activeEnrollments = await context.ClassEnrollments
            .Where(e => e.ClassId == session.ClassId && e.Status == EnrollmentStatus.Active)
            .ToListAsync(cancellationToken);

        var existingAssignments = await context.StudentSessionAssignments
            .Where(a => a.SessionId == session.Id)
            .ToListAsync(cancellationToken);

        var assignmentsByEnrollmentId = existingAssignments.ToDictionary(a => a.ClassEnrollmentId);
        var activeEnrollmentIds = activeEnrollments.Select(e => e.Id).ToHashSet();
        var now = DateTime.UtcNow;

        foreach (var enrollment in activeEnrollments)
        {
            var shouldBeAssigned =
                session.Status != SessionStatus.Cancelled &&
                DateOnly.FromDateTime(session.PlannedDatetime) >= enrollment.EnrollDate &&
                MatchesSelectionPattern(session, enrollment.SessionSelectionPattern);

            assignmentsByEnrollmentId.TryGetValue(enrollment.Id, out var assignment);

            if (shouldBeAssigned)
            {
                if (assignment == null)
                {
                    context.StudentSessionAssignments.Add(new StudentSessionAssignment
                    {
                        Id = Guid.NewGuid(),
                        SessionId = session.Id,
                        StudentProfileId = enrollment.StudentProfileId,
                        ClassEnrollmentId = enrollment.Id,
                        RegistrationId = enrollment.RegistrationId,
                        Track = enrollment.Track,
                        Status = StudentSessionAssignmentStatus.Assigned,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }
                else
                {
                    assignment.StudentProfileId = enrollment.StudentProfileId;
                    assignment.RegistrationId = enrollment.RegistrationId;
                    assignment.Track = enrollment.Track;
                    assignment.Status = StudentSessionAssignmentStatus.Assigned;
                    assignment.UpdatedAt = now;
                }
            }
            else if (assignment is not null && assignment.Status != StudentSessionAssignmentStatus.Cancelled)
            {
                assignment.Status = StudentSessionAssignmentStatus.Cancelled;
                assignment.UpdatedAt = now;
            }
        }

        foreach (var orphanedAssignment in existingAssignments.Where(a => !activeEnrollmentIds.Contains(a.ClassEnrollmentId)))
        {
            if (orphanedAssignment.Status != StudentSessionAssignmentStatus.Cancelled)
            {
                orphanedAssignment.Status = StudentSessionAssignmentStatus.Cancelled;
                orphanedAssignment.UpdatedAt = now;
            }
        }
    }

    public async Task CancelFutureAssignmentsForEnrollmentAsync(
        Guid enrollmentId,
        DateTime effectiveFromUtc,
        CancellationToken cancellationToken)
    {
        var assignments = await context.StudentSessionAssignments
            .Include(a => a.Session)
            .Where(a => a.ClassEnrollmentId == enrollmentId
                && a.Status == StudentSessionAssignmentStatus.Assigned
                && a.Session.PlannedDatetime >= effectiveFromUtc)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var assignment in assignments)
        {
            assignment.Status = StudentSessionAssignmentStatus.Cancelled;
            assignment.UpdatedAt = now;
        }
    }

    public async Task CancelAssignmentsForEnrollmentInRangeAsync(
        Guid enrollmentId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken)
    {
        var assignments = await context.StudentSessionAssignments
            .Include(a => a.Session)
            .Where(a => a.ClassEnrollmentId == enrollmentId
                && a.Status == StudentSessionAssignmentStatus.Assigned
                && a.Session.PlannedDatetime >= fromUtc
                && a.Session.PlannedDatetime <= toUtc)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var assignment in assignments)
        {
            assignment.Status = StudentSessionAssignmentStatus.Cancelled;
            assignment.UpdatedAt = now;
        }
    }

    public async Task ReassignFutureAssignmentsToRegistrationAsync(
        Guid oldRegistrationId,
        Guid newRegistrationId,
        DateTime effectiveFromUtc,
        CancellationToken cancellationToken)
    {
        var assignments = await context.StudentSessionAssignments
            .Include(a => a.Session)
            .Where(a => a.RegistrationId == oldRegistrationId
                && a.Status == StudentSessionAssignmentStatus.Assigned
                && a.Session.PlannedDatetime >= effectiveFromUtc)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var assignment in assignments)
        {
            assignment.RegistrationId = newRegistrationId;
            assignment.UpdatedAt = now;
        }
    }

    public async Task<bool> HasRegularAssignmentsAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        return await context.StudentSessionAssignments
            .AnyAsync(a => a.SessionId == sessionId, cancellationToken);
    }

    public async Task<bool> IsStudentRegularlyAssignedToSessionAsync(
        Guid sessionId,
        Guid studentProfileId,
        CancellationToken cancellationToken)
    {
        var participants = await GetRegularParticipantsAsync(sessionId, cancellationToken);
        return participants.Any(p => p.StudentProfileId == studentProfileId);
    }

    public async Task<int> GetRegularAssignedStudentCountAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var participants = await GetRegularParticipantsAsync(sessionId, cancellationToken);
        return participants.Count;
    }

    public async Task<List<RegularSessionParticipant>> GetRegularParticipantsAsync(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var hasAssignments = await HasRegularAssignmentsAsync(sessionId, cancellationToken);

        if (hasAssignments)
        {
            return await context.StudentSessionAssignments
                .AsNoTracking()
                .Where(a => a.SessionId == sessionId && a.Status == StudentSessionAssignmentStatus.Assigned)
                .Select(a => new RegularSessionParticipant(
                    a.StudentProfileId,
                    a.ClassEnrollmentId,
                    a.RegistrationId,
                    a.Track))
                .ToListAsync(cancellationToken);
        }

        var sessionInfo = await context.Sessions
            .AsNoTracking()
            .Where(s => s.Id == sessionId)
            .Select(s => new
            {
                s.ClassId,
                SessionDate = DateOnly.FromDateTime(s.PlannedDatetime)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (sessionInfo == null)
        {
            return new List<RegularSessionParticipant>();
        }

        return await context.ClassEnrollments
            .AsNoTracking()
            .Where(e => e.ClassId == sessionInfo.ClassId
                && e.Status == EnrollmentStatus.Active
                && e.EnrollDate <= sessionInfo.SessionDate)
            .Select(e => new RegularSessionParticipant(
                e.StudentProfileId,
                e.Id,
                e.RegistrationId,
                e.Track))
            .ToListAsync(cancellationToken);
    }

    private bool MatchesSelectionPattern(Session session, string? sessionSelectionPattern)
    {
        if (string.IsNullOrWhiteSpace(sessionSelectionPattern))
        {
            return true;
        }

        var sessionDate = DateOnly.FromDateTime(session.PlannedDatetime);
        var parseResult = patternParser.ParseAndGenerateOccurrences(
            sessionSelectionPattern,
            sessionDate,
            sessionDate);

        if (parseResult.IsFailure)
        {
            return false;
        }

        return parseResult.Value.Any(occurrence =>
            Math.Abs((occurrence - session.PlannedDatetime).TotalMinutes) < 1);
    }

    private static long ToMinuteKey(DateTime value)
    {
        return value.ToUniversalTime().Ticks / TimeSpan.TicksPerMinute;
    }
}
