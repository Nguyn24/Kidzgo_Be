using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public readonly record struct SessionParticipant(
    Guid StudentProfileId,
    Guid? ClassEnrollmentId,
    Guid? RegistrationId,
    RegistrationTrackType? Track,
    bool IsMakeup,
    Guid? MakeupCreditId);

public sealed class SessionParticipantService(
    IDbContext context,
    StudentSessionAssignmentService studentSessionAssignmentService)
{
    public async Task<List<SessionParticipant>> GetParticipantsAsync(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var participants = new Dictionary<Guid, SessionParticipant>();

        foreach (var regularParticipant in await studentSessionAssignmentService
                     .GetRegularParticipantsAsync(sessionId, cancellationToken))
        {
            participants[regularParticipant.StudentProfileId] = new SessionParticipant(
                regularParticipant.StudentProfileId,
                regularParticipant.ClassEnrollmentId,
                regularParticipant.RegistrationId,
                regularParticipant.Track,
                false,
                null);
        }

        var makeupParticipants = await context.MakeupAllocations
            .AsNoTracking()
            .Where(a => a.TargetSessionId == sessionId && a.Status != MakeupAllocationStatus.Cancelled)
            .Select(a => new SessionParticipant(
                a.MakeupCredit.StudentProfileId,
                null,
                null,
                null,
                true,
                a.MakeupCreditId))
            .ToListAsync(cancellationToken);

        foreach (var makeupParticipant in makeupParticipants)
        {
            if (participants.TryGetValue(makeupParticipant.StudentProfileId, out var existingParticipant))
            {
                participants[makeupParticipant.StudentProfileId] = existingParticipant with
                {
                    IsMakeup = true,
                    MakeupCreditId = makeupParticipant.MakeupCreditId
                };
                continue;
            }

            participants[makeupParticipant.StudentProfileId] = makeupParticipant;
        }

        return participants.Values.ToList();
    }

    public async Task<Result> EnsureStudentAssignedToSessionAsync(
        Guid sessionId,
        Guid studentProfileId,
        CancellationToken cancellationToken)
    {
        var participants = await GetParticipantsAsync(sessionId, cancellationToken);
        if (participants.Any(p => p.StudentProfileId == studentProfileId))
        {
            return Result.Success();
        }

        return Result.Failure(Error.Validation(
            "Session.StudentNotAssigned",
            $"Student '{studentProfileId}' is not assigned to session '{sessionId}'."));
    }

    public async Task<List<(DateTime Start, DateTime End)>> GetStudentBookedSlotsAsync(
        Guid studentProfileId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken)
    {
        var regularAssignments = await context.StudentSessionAssignments
            .AsNoTracking()
            .Where(a => a.StudentProfileId == studentProfileId
                && a.Status == StudentSessionAssignmentStatus.Assigned
                && a.Session.Status != SessionStatus.Cancelled
                && a.Session.PlannedDatetime >= fromUtc
                && a.Session.PlannedDatetime <= toUtc)
            .Select(a => new
            {
                Start = a.Session.PlannedDatetime,
                End = a.Session.PlannedDatetime.AddMinutes(a.Session.DurationMinutes)
            })
            .ToListAsync(cancellationToken);

        var legacySessions = await context.Sessions
            .AsNoTracking()
            .Where(s => s.Status == SessionStatus.Scheduled
                && s.PlannedDatetime >= fromUtc
                && s.PlannedDatetime <= toUtc
                && !context.StudentSessionAssignments.Any(a => a.SessionId == s.Id)
                && s.Class.ClassEnrollments.Any(ce =>
                    ce.StudentProfileId == studentProfileId &&
                    ce.Status == Domain.Classes.EnrollmentStatus.Active &&
                    ce.EnrollDate <= DateOnly.FromDateTime(s.PlannedDatetime)))
            .Select(s => new
            {
                Start = s.PlannedDatetime,
                End = s.PlannedDatetime.AddMinutes(s.DurationMinutes)
            })
            .ToListAsync(cancellationToken);

        var makeupSlots = await context.MakeupAllocations
            .AsNoTracking()
            .Where(a => a.MakeupCredit.StudentProfileId == studentProfileId
                && a.Status != MakeupAllocationStatus.Cancelled
                && a.TargetSession.Status == SessionStatus.Scheduled
                && a.TargetSession.PlannedDatetime >= fromUtc
                && a.TargetSession.PlannedDatetime <= toUtc)
            .Select(a => new
            {
                Start = a.TargetSession.PlannedDatetime,
                End = a.TargetSession.PlannedDatetime.AddMinutes(a.TargetSession.DurationMinutes)
            })
            .ToListAsync(cancellationToken);

        return regularAssignments
            .Concat(legacySessions)
            .Concat(makeupSlots)
            .Select(x => (x.Start, x.End))
            .Distinct()
            .ToList();
    }
}
