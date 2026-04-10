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

public readonly record struct StudentBookedSlot(
    DateTime Start,
    DateTime End,
    Guid? ClassEnrollmentId,
    Guid? ClassId,
    string? ClassCode,
    string? ClassTitle,
    bool IsMakeup);

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
        var detailedSlots = await GetStudentBookedSlotsDetailedAsync(
            studentProfileId,
            fromUtc,
            toUtc,
            cancellationToken);

        return detailedSlots
            .Select(x => (x.Start, x.End))
            .Distinct()
            .ToList();
    }

    public async Task<List<StudentBookedSlot>> GetStudentBookedSlotsDetailedAsync(
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
            .Select(a => new StudentBookedSlot(
                a.Session.PlannedDatetime,
                a.Session.PlannedDatetime.AddMinutes(a.Session.DurationMinutes),
                a.ClassEnrollmentId,
                a.Session.ClassId,
                a.Session.Class.Code,
                a.Session.Class.Title,
                false))
            .ToListAsync(cancellationToken);

        var activeEnrollDatesByClass = await context.ClassEnrollments
            .AsNoTracking()
            .Where(ce => ce.StudentProfileId == studentProfileId
                && ce.Status == Domain.Classes.EnrollmentStatus.Active)
            .GroupBy(ce => ce.ClassId)
            .Select(g => new
            {
                ClassId = g.Key,
                EnrollDates = g.Select(ce => ce.EnrollDate).ToList()
            })
            .ToListAsync(cancellationToken);

        var enrollDateLookup = activeEnrollDatesByClass.ToDictionary(x => x.ClassId, x => x.EnrollDates);

        var legacySessions = await context.Sessions
            .AsNoTracking()
            .Where(s => s.Status == SessionStatus.Scheduled
                && s.PlannedDatetime >= fromUtc
                && s.PlannedDatetime <= toUtc
                && !context.StudentSessionAssignments.Any(a => a.SessionId == s.Id))
            .Select(s => new
            {
                s.ClassId,
                s.Class.Code,
                s.Class.Title,
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
            .Select(a => new StudentBookedSlot(
                a.TargetSession.PlannedDatetime,
                a.TargetSession.PlannedDatetime.AddMinutes(a.TargetSession.DurationMinutes),
                null,
                a.TargetSession.ClassId,
                a.TargetSession.Class.Code,
                a.TargetSession.Class.Title,
                true))
            .ToListAsync(cancellationToken);

        var filteredLegacySessions = legacySessions
            .Where(s =>
                enrollDateLookup.TryGetValue(s.ClassId, out var enrollDates) &&
                enrollDates.Any(enrollDate => enrollDate <= VietnamTime.ToVietnamDateOnly(s.Start)))
            .Select(s => new StudentBookedSlot(
                s.Start,
                s.End,
                null,
                s.ClassId,
                s.Code,
                s.Title,
                false));

        return regularAssignments
            .Concat(filteredLegacySessions)
            .Concat(makeupSlots)
            .Distinct()
            .ToList();
    }
}
