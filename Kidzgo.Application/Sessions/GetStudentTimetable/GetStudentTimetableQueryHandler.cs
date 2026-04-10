using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Registrations;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.GetStudentTimetable;

public sealed class GetStudentTimetableQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetStudentTimetableQuery, GetStudentTimetableResponse>
{
    public async Task<Result<GetStudentTimetableResponse>> Handle(GetStudentTimetableQuery query, CancellationToken cancellationToken)
    {
        // Get StudentId from context (token)
        var studentId = userContext.StudentId;

        if (!studentId.HasValue)
        {
            return Result.Failure<GetStudentTimetableResponse>(ProfileErrors.StudentNotFound);
        }

        // Verify student exists and is active
        var profile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == studentId.Value, cancellationToken);

        if (profile == null)
        {
            return Result.Failure<GetStudentTimetableResponse>(ProfileErrors.NotFound(studentId.Value));
        }

        if (profile.ProfileType != Domain.Users.ProfileType.Student)
        {
            return Result.Failure<GetStudentTimetableResponse>(ProfileErrors.StudentNotFound);
        }

        if (!profile.IsActive || profile.IsDeleted)
        {
            return Result.Failure<GetStudentTimetableResponse>(ProfileErrors.StudentNotFound);
        }

        // Verify the student belongs to the current user
        if (profile.UserId != userContext.UserId)
        {
            return Result.Failure<GetStudentTimetableResponse>(ProfileErrors.StudentNotFound);
        }

        DateTime? fromUtc = null;
        if (query.From.HasValue)
        {
            fromUtc = VietnamTime.NormalizeToUtc(query.From.Value);
        }

        DateTime? toUtc = null;
        if (query.To.HasValue)
        {
            var normalizedToUtc = VietnamTime.NormalizeToUtc(query.To.Value);
            toUtc = VietnamTime.EndOfVietnamDayUtc(normalizedToUtc);
        }

        var regularAssignmentsQuery = context.StudentSessionAssignments
            .AsNoTracking()
            .Where(a => a.StudentProfileId == studentId.Value
                && a.Status == StudentSessionAssignmentStatus.Assigned
                && a.Session.Status != SessionStatus.Cancelled);

        if (fromUtc.HasValue)
        {
            regularAssignmentsQuery = regularAssignmentsQuery
                .Where(a => a.Session.PlannedDatetime >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            regularAssignmentsQuery = regularAssignmentsQuery
                .Where(a => a.Session.PlannedDatetime <= toUtc.Value);
        }

        var regularAssignments = await regularAssignmentsQuery
            .Select(a => new
            {
                a.SessionId,
                a.ClassEnrollmentId,
                a.RegistrationId,
                a.Track
            })
            .ToListAsync(cancellationToken);

        var studentActiveEnrollmentsQuery = context.ClassEnrollments
            .AsNoTracking()
            .Where(ce => ce.StudentProfileId == studentId.Value
                && ce.Status == EnrollmentStatus.Active);

        var enrollmentsWithAssignmentIds = await context.StudentSessionAssignments
            .AsNoTracking()
            .Where(a => a.StudentProfileId == studentId.Value)
            .Select(a => a.ClassEnrollmentId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var enrollmentsWithAssignmentIdSet = enrollmentsWithAssignmentIds.ToHashSet();

        var legacyEnrollments = await studentActiveEnrollmentsQuery
            .Where(ce => !enrollmentsWithAssignmentIdSet.Contains(ce.Id))
            .Select(ce => new
            {
                ce.Id,
                ce.ClassId,
                ce.EnrollDate,
                ce.RegistrationId,
                ce.Track
            })
            .ToListAsync(cancellationToken);

        var legacyClassIds = legacyEnrollments
            .Select(ce => ce.ClassId)
            .Distinct()
            .ToList();

        var legacySessionsQuery = context.Sessions
            .AsNoTracking()
            .Where(s => s.Status != SessionStatus.Cancelled
                && legacyClassIds.Contains(s.ClassId));

        if (fromUtc.HasValue)
        {
            legacySessionsQuery = legacySessionsQuery.Where(s => s.PlannedDatetime >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            legacySessionsQuery = legacySessionsQuery.Where(s => s.PlannedDatetime <= toUtc.Value);
        }

        var legacySessions = await legacySessionsQuery
            .ToListAsync(cancellationToken);

        var legacyAssignments = legacySessions
            .SelectMany(session => legacyEnrollments
                .Where(enrollment =>
                    enrollment.ClassId == session.ClassId &&
                    enrollment.EnrollDate <= VietnamTime.ToVietnamDateOnly(session.PlannedDatetime))
                .Select(enrollment => new
                {
                    SessionId = session.Id,
                    enrollment.RegistrationId,
                    enrollment.Track
                }))
            .GroupBy(x => x.SessionId)
            .Select(group => group
                .OrderByDescending(x => x.RegistrationId.HasValue)
                .First())
            .ToList();

        var makeupAssignmentsQuery = context.MakeupAllocations
            .AsNoTracking()
            .Where(a => a.MakeupCredit.StudentProfileId == studentId.Value
                && a.Status != MakeupAllocationStatus.Cancelled
                && a.TargetSession.Status != SessionStatus.Cancelled);

        if (fromUtc.HasValue)
        {
            makeupAssignmentsQuery = makeupAssignmentsQuery
                .Where(a => a.TargetSession.PlannedDatetime >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            makeupAssignmentsQuery = makeupAssignmentsQuery
                .Where(a => a.TargetSession.PlannedDatetime <= toUtc.Value);
        }

        var makeupAssignments = await makeupAssignmentsQuery
            .Select(a => new
            {
                SessionId = a.TargetSessionId
            })
            .ToListAsync(cancellationToken);

        var sessionMetadata = new Dictionary<Guid, (Guid? RegistrationId, string? Track, bool IsMakeup)>();

        foreach (var assignment in regularAssignments)
        {
            sessionMetadata[assignment.SessionId] = (
                assignment.RegistrationId,
                RegistrationTrackHelper.ToTrackName(assignment.Track),
                false);
        }

        foreach (var legacyAssignment in legacyAssignments)
        {
            sessionMetadata.TryAdd(
                legacyAssignment.SessionId,
                (
                    legacyAssignment.RegistrationId,
                    RegistrationTrackHelper.ToTrackName(legacyAssignment.Track),
                    false));
        }

        foreach (var makeupAssignment in makeupAssignments)
        {
            if (sessionMetadata.TryGetValue(makeupAssignment.SessionId, out var existingMetadata))
            {
                sessionMetadata[makeupAssignment.SessionId] = (
                    existingMetadata.RegistrationId,
                    existingMetadata.Track,
                    true);
                continue;
            }

            sessionMetadata[makeupAssignment.SessionId] = (null, null, true);
        }

        var sessionIds = sessionMetadata.Keys.ToList();
        if (sessionIds.Count == 0)
        {
            return Result.Success(new GetStudentTimetableResponse());
        }

        var sessionDetails = await context.Sessions
            .AsNoTracking()
            .Where(s => sessionIds.Contains(s.Id))
            .Select(s => new TimetableItemDto
            {
                Id = s.Id,
                Color = s.Color,
                ClassId = s.ClassId,
                ClassCode = s.Class.Code,
                ClassTitle = s.Class.Title,
                PlannedDatetime = s.PlannedDatetime,
                ActualDatetime = s.ActualDatetime,
                DurationMinutes = s.DurationMinutes,
                ParticipationType = s.ParticipationType.ToString(),
                Status = s.Status.ToString(),
                PlannedRoomId = s.PlannedRoomId,
                PlannedRoomName = s.PlannedRoom != null ? s.PlannedRoom.Name : null,
                ActualRoomId = s.ActualRoomId,
                ActualRoomName = s.ActualRoom != null ? s.ActualRoom.Name : null,
                PlannedTeacherId = s.PlannedTeacherId,
                PlannedTeacherName = s.PlannedTeacher != null ? s.PlannedTeacher.Name : null,
                ActualTeacherId = s.ActualTeacherId,
                ActualTeacherName = s.ActualTeacher != null ? s.ActualTeacher.Name : null,
                PlannedAssistantId = s.PlannedAssistantId,
                PlannedAssistantName = s.PlannedAssistant != null ? s.PlannedAssistant.Name : null,
                LessonPlanId = s.LessonPlan != null ? s.LessonPlan.Id : null,
                LessonPlanLink = s.LessonPlan != null ? $"/api/lesson-plans/{s.LessonPlan.Id}" : null,
                AttendanceStatus = s.Attendances
                    .Where(a => a.StudentProfileId == studentId.Value)
                    .Select(a => a.AttendanceStatus.ToString())
                    .FirstOrDefault(),
                AbsenceType = s.Attendances
                    .Where(a => a.StudentProfileId == studentId.Value)
                    .Select(a => a.AbsenceType.ToString())
                    .FirstOrDefault(),
                AttendanceMarkedAt = s.Attendances
                    .Where(a => a.StudentProfileId == studentId.Value)
                    .Select(a => a.MarkedAt)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var sessions = sessionDetails
            .Select(s =>
            {
                var metadata = sessionMetadata[s.Id];
                return new TimetableItemDto
                {
                    Id = s.Id,
                    Color = s.Color,
                    ClassId = s.ClassId,
                    ClassCode = s.ClassCode,
                    ClassTitle = s.ClassTitle,
                    PlannedDatetime = s.PlannedDatetime,
                    ActualDatetime = s.ActualDatetime,
                    DurationMinutes = s.DurationMinutes,
                    ParticipationType = s.ParticipationType,
                    Status = s.Status,
                    PlannedRoomId = s.PlannedRoomId,
                    PlannedRoomName = s.PlannedRoomName,
                    ActualRoomId = s.ActualRoomId,
                    ActualRoomName = s.ActualRoomName,
                    PlannedTeacherId = s.PlannedTeacherId,
                    PlannedTeacherName = s.PlannedTeacherName,
                    ActualTeacherId = s.ActualTeacherId,
                    ActualTeacherName = s.ActualTeacherName,
                    PlannedAssistantId = s.PlannedAssistantId,
                    PlannedAssistantName = s.PlannedAssistantName,
                    LessonPlanId = s.LessonPlanId,
                    LessonPlanLink = s.LessonPlanLink,
                    RegistrationId = metadata.RegistrationId,
                    Track = metadata.Track,
                    IsMakeup = metadata.IsMakeup,
                    AttendanceStatus = s.AttendanceStatus,
                    AbsenceType = s.AbsenceType,
                    AttendanceMarkedAt = s.AttendanceMarkedAt
                };
            })
            .OrderBy(s => s.PlannedDatetime)
            .ToList();

        return Result.Success(new GetStudentTimetableResponse
        {
            Sessions = sessions
        });
    }
}

