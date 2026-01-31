using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
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

        var attendance = await context.Attendances
            .FirstOrDefaultAsync(a => a.SessionId == command.SessionId && a.StudentProfileId == command.StudentProfileId,
                cancellationToken);

        if (attendance is null)
        {
            attendance = new Domain.Sessions.Attendance
            {
                Id = Guid.NewGuid(),
                SessionId = command.SessionId,
                StudentProfileId = command.StudentProfileId,
            };
            context.Attendances.Add(attendance);
        }

        if (command.Note is not null)
        {
            attendance.Note = command.Note;
        }

        attendance.AttendanceStatus = command.AttendanceStatus;

        if (command.AttendanceStatus == AttendanceStatus.Absent)
        {
            var absenceType = await ResolveAbsenceType(command.StudentProfileId, session, cancellationToken);
            attendance.AbsenceType = absenceType;

            // Create makeup credit for WITH_NOTICE_24H
            if (absenceType == AbsenceType.WithNotice24H)
            {
                bool creditExists = await context.MakeupCredits
                    .AnyAsync(c => c.StudentProfileId == command.StudentProfileId &&
                                   c.SourceSessionId == session.Id &&
                                   c.CreatedReason == CreatedReason.ApprovedLeave24H,
                        cancellationToken);

                if (!creditExists)
                {
                    var credit = new MakeupCredit
                    {
                        Id = Guid.NewGuid(),
                        StudentProfileId = command.StudentProfileId,
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
        else
        {
            attendance.AbsenceType = null;
        }

        attendance.MarkedBy = userContext.UserId;
        attendance.MarkedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new MarkAttendanceResponse
        {
            Id = attendance.Id,
            SessionId = attendance.SessionId,
            StudentProfileId = attendance.StudentProfileId,
            AttendanceStatus = attendance.AttendanceStatus.ToString(),
            AbsenceType = attendance.AbsenceType.HasValue ? attendance.AbsenceType.Value.ToString() : null,
            MarkedAt = attendance.MarkedAt,
            Note = attendance.Note
        };
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

