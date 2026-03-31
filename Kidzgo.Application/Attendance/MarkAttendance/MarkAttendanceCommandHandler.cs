using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Attendance.MarkAttendance;

public sealed class MarkAttendanceCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<MarkAttendanceCommand, MarkAttendanceResponse>
{
    public async Task<Result<MarkAttendanceResponse>> Handle(MarkAttendanceCommand command, CancellationToken cancellationToken)
    {
        var session = await context.Sessions.FirstOrDefaultAsync(s => s.Id == command.SessionId, cancellationToken);
        if (session is null)
        {
            return Result.Failure<MarkAttendanceResponse>(AttendanceErrors.NotFound(command.SessionId));
        }

        var results = new List<AttendanceResultItem>();

        foreach (var item in command.Attendances)
        {
            var attendance = await context.Attendances
                .FirstOrDefaultAsync(a => a.SessionId == command.SessionId && a.StudentProfileId == item.StudentProfileId,
                    cancellationToken);

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
                            CreatedAt = DateTime.UtcNow
                        };
                        context.MakeupCredits.Add(credit);
                    }
                }
            }
            else if (item.AttendanceStatus == AttendanceStatus.Present)
            {
                // Update UsedSessions and RemainingSessions when student is present
                await UpdateRegistrationSessionsAsync(item.StudentProfileId, session.ClassId, cancellationToken);
                attendance.AbsenceType = null;
            }
            else
            {
                attendance.AbsenceType = null;
            }

            attendance.MarkedBy = userContext.UserId;
            attendance.MarkedAt = DateTime.UtcNow;

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

        return Result.Success(new MarkAttendanceResponse()
        {
            Results = results
        });
    }

    private async Task UpdateRegistrationSessionsAsync(Guid studentProfileId, Guid? classId, CancellationToken cancellationToken)
    {
        if (classId == null) return;

        var enrollment = await context.ClassEnrollments
            .Include(ce => ce.Registration)
            .FirstOrDefaultAsync(ce => ce.ClassId == classId
                && ce.StudentProfileId == studentProfileId
                && ce.Status == EnrollmentStatus.Active,
                cancellationToken);

        var registration = enrollment?.Registration;
        if (registration is null)
        {
            registration = await context.Registrations
                .FirstOrDefaultAsync(r => r.StudentProfileId == studentProfileId
                    && (r.ClassId == classId || r.SecondaryClassId == classId)
                    && r.Status != RegistrationStatus.Completed
                    && r.Status != RegistrationStatus.Cancelled,
                    cancellationToken);
        }

        if (registration != null && registration.RemainingSessions > 0)
        {
            registration.UsedSessions++;
            registration.RemainingSessions--;
            registration.UpdatedAt = DateTime.UtcNow;

            // Auto complete registration when all sessions are used
            if (registration.RemainingSessions == 0)
            {
                registration.Status = RegistrationStatus.Completed;
                registration.UpdatedAt = DateTime.UtcNow;
            }

            // Check if all students in class have completed their sessions
            await CheckAndUpdateClassCompletionAsync(classId.Value, cancellationToken);
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
                classEntity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    private async Task<AbsenceType> ResolveAbsenceType(Guid studentProfileId, Session session, CancellationToken cancellationToken)
    {
        var leave = await context.LeaveRequests
            .Where(l => l.StudentProfileId == studentProfileId
                        && l.SessionDate <= DateOnly.FromDateTime(session.PlannedDatetime)
                        && (l.EndDate == null || l.EndDate >= DateOnly.FromDateTime(session.PlannedDatetime))
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

