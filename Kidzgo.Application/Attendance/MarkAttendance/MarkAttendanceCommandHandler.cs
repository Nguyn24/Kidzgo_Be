using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Missions.Shared;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Attendance.MarkAttendance;

public sealed class MarkAttendanceCommandHandler(
    IDbContext context,
    IUserContext userContext,
    IGamificationService gamificationService,
    SessionParticipantService sessionParticipantService)
    : ICommandHandler<MarkAttendanceCommand, MarkAttendanceResponse>
{
    public async Task<Result<MarkAttendanceResponse>> Handle(MarkAttendanceCommand command, CancellationToken cancellationToken)
    {
        var session = await context.Sessions.FirstOrDefaultAsync(s => s.Id == command.SessionId, cancellationToken);
        if (session is null)
        {
            return Result.Failure<MarkAttendanceResponse>(AttendanceErrors.NotFound(command.SessionId));
        }

        var sessionDate = VietnamTime.ToVietnamDateOnly(session.ActualDatetime ?? session.PlannedDatetime);
        var today = VietnamTime.TodayDateOnly();
        if (!command.IsAdmin && sessionDate > today)
        {
            return Result.Failure<MarkAttendanceResponse>(
                AttendanceErrors.FutureSessionNotAllowed(command.SessionId));
        }

        var results = new List<AttendanceResultItem>();
        var studentsWithClassAttendanceMissionChanges = new HashSet<Guid>();
        var participants = await sessionParticipantService.GetParticipantsAsync(command.SessionId, cancellationToken);
        var participantsByStudent = participants.ToDictionary(p => p.StudentProfileId);

        foreach (var item in command.Attendances)
        {
            if (!participantsByStudent.TryGetValue(item.StudentProfileId, out var participant))
            {
                return Result.Failure<MarkAttendanceResponse>(Error.Validation(
                    "Attendance.StudentNotAssigned",
                    $"Student '{item.StudentProfileId}' is not assigned to session '{command.SessionId}'."));
            }

            var attendance = await context.Attendances
                .FirstOrDefaultAsync(a => a.SessionId == command.SessionId && a.StudentProfileId == item.StudentProfileId,
                    cancellationToken);

            var hadExistingAttendance = attendance is not null;
            var previousStatus = attendance?.AttendanceStatus;
            var shouldTrackClassAttendanceMission = previousStatus != item.AttendanceStatus;

            if (attendance is null)
            {
                attendance = new Domain.Sessions.Attendance
                {
                    Id = Guid.NewGuid(),
                    SessionId = command.SessionId,
                    StudentProfileId = item.StudentProfileId,
                };
                context.Attendances.Add(attendance);
            }

            if (item.Note is not null)
            {
                attendance.Note = item.Note;
            }

            attendance.AttendanceStatus = item.AttendanceStatus;

            if (item.AttendanceStatus == AttendanceStatus.Absent)
            {
                var absenceType = await ResolveAbsenceType(item.StudentProfileId, session, cancellationToken);
                attendance.AbsenceType = absenceType;

                if (absenceType == AbsenceType.WithNotice24H)
                {
                    bool creditExists = await context.MakeupCredits
                        .AnyAsync(c => c.StudentProfileId == item.StudentProfileId &&
                                       c.SourceSessionId == session.Id &&
                                       c.CreatedReason == CreatedReason.ApprovedLeave24H,
                            cancellationToken);

                    if (!creditExists)
                    {
                        var credit = new MakeupCredit
                        {
                            Id = Guid.NewGuid(),
                            StudentProfileId = item.StudentProfileId,
                            SourceSessionId = session.Id,
                            Status = MakeupCreditStatus.Available,
                            CreatedReason = CreatedReason.ApprovedLeave24H,
                            ExpiresAt = null,
                            CreatedAt = VietnamTime.UtcNow()
                        };
                        context.MakeupCredits.Add(credit);
                    }
                }
            }
            else if (item.AttendanceStatus == AttendanceStatus.Present)
            {
                if (!participant.IsMakeup && (!hadExistingAttendance || previousStatus != AttendanceStatus.Present))
                {
                    await UpdateRegistrationSessionsAsync(participant.RegistrationId, cancellationToken);
                }
                attendance.AbsenceType = null;
            }
            else
            {
                attendance.AbsenceType = null;
            }

            attendance.MarkedBy = userContext.UserId;
            attendance.MarkedAt = VietnamTime.UtcNow();

            if (shouldTrackClassAttendanceMission)
            {
                studentsWithClassAttendanceMissionChanges.Add(item.StudentProfileId);
            }

            results.Add(new AttendanceResultItem
            {
                Id = attendance.Id,
                SessionId = attendance.SessionId,
                StudentProfileId = attendance.StudentProfileId,
                AttendanceStatus = attendance.AttendanceStatus.ToString(),
                AbsenceType = attendance.AbsenceType.HasValue ? attendance.AbsenceType.Value.ToString() : null,
                MarkedAt = attendance.MarkedAt,
                Note = attendance.Note
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        foreach (var studentProfileId in studentsWithClassAttendanceMissionChanges)
        {
            await ClassAttendanceMissionProgressTracker.TrackAsync(
                context,
                gamificationService,
                studentProfileId,
                session,
                cancellationToken);
        }

        return Result.Success(new MarkAttendanceResponse()
        {
            Results = results
        });
    }

    private async Task UpdateRegistrationSessionsAsync(Guid? registrationId, CancellationToken cancellationToken)
    {
        if (!registrationId.HasValue)
        {
            return;
        }

        var registration = await context.Registrations
            .FirstOrDefaultAsync(r => r.Id == registrationId.Value, cancellationToken);

        if (registration != null && registration.RemainingSessions > 0)
        {
            registration.UsedSessions++;
            registration.RemainingSessions--;
            registration.UpdatedAt = VietnamTime.UtcNow();

            // Auto complete registration when all sessions are used
            if (registration.RemainingSessions == 0)
            {
                registration.Status = RegistrationStatus.Completed;
                registration.UpdatedAt = VietnamTime.UtcNow();
            }

            // Check if all students in class have completed their sessions
            if (registration.ClassId.HasValue)
            {
                await CheckAndUpdateClassCompletionAsync(registration.ClassId.Value, cancellationToken);
            }

            if (registration.SecondaryClassId.HasValue)
            {
                await CheckAndUpdateClassCompletionAsync(registration.SecondaryClassId.Value, cancellationToken);
            }
        }
    }

    private async Task CheckAndUpdateClassCompletionAsync(Guid classId, CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .Include(c => c.ClassEnrollments)
            .FirstOrDefaultAsync(c => c.Id == classId, cancellationToken);

        if (classEntity == null) return;

        // Get all active registrations for this class
        var activeRegistrations = await context.Registrations
            .Where(r => (r.ClassId == classId || r.SecondaryClassId == classId)
                && r.Status == RegistrationStatus.Studying)
            .ToListAsync(cancellationToken);

        // If no active registrations, check if class should be completed
        if (activeRegistrations.Count == 0)
        {
            // Check if there are any active enrollments
            var activeEnrollments = classEntity.ClassEnrollments
                .Count(ce => ce.Status == EnrollmentStatus.Active);

            if (activeEnrollments == 0 && classEntity.Status == ClassStatus.Active)
            {
                classEntity.Status = ClassStatus.Completed;
                classEntity.UpdatedAt = VietnamTime.UtcNow();
            }
        }
    }

    private async Task<AbsenceType> ResolveAbsenceType(Guid studentProfileId, Session session, CancellationToken cancellationToken)
    {
        var sessionDate = VietnamTime.ToVietnamDateOnly(session.PlannedDatetime);
        var leave = await context.LeaveRequests
            .Where(l => l.StudentProfileId == studentProfileId
                        && (l.SessionId == session.Id ||
                            (l.SessionDate <= sessionDate
                             && (l.EndDate == null || l.EndDate >= sessionDate)))
                        && l.Status == LeaveRequestStatus.Approved)
            .OrderByDescending(l => l.RequestedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (leave is not null)
        {
            if (leave.NoticeHours.GetValueOrDefault() >= 24)
            {
                return AbsenceType.WithNotice24H;
            }

            if (leave.NoticeHours.GetValueOrDefault() >= 0)
            {
                return AbsenceType.Under24H;
            }
        }

        return AbsenceType.NoNotice;
    }
}

