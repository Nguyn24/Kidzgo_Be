using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Audit;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kidzgo.Application.Attendance.UpdateAttendance;

public sealed class UpdateAttendanceCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<UpdateAttendanceCommand, UpdateAttendanceResponse>
{
    public async Task<Result<UpdateAttendanceResponse>> Handle(UpdateAttendanceCommand request, CancellationToken cancellationToken)
    {
        var attendance = await context.Attendances
            .Include(a => a.Session)
            .FirstOrDefaultAsync(a =>
                a.SessionId == request.SessionId &&
                a.StudentProfileId == request.StudentProfileId,
                cancellationToken);

        if (attendance is null)
        {
            return Result.Failure<UpdateAttendanceResponse>(
                AttendanceErrors.NotFoundForSessionStudent(request.SessionId, request.StudentProfileId));
        }

        var sessionDate = VietnamTime.ToVietnamDateOnly(attendance.Session.ActualDatetime ?? attendance.Session.PlannedDatetime);
        var today = VietnamTime.TodayDateOnly();
        if (!request.IsAdmin && sessionDate > today)
        {
            return Result.Failure<UpdateAttendanceResponse>(
                AttendanceErrors.FutureSessionNotAllowed(attendance.SessionId));
        }

        var sessionEndUtc = (attendance.Session.ActualDatetime ?? attendance.Session.PlannedDatetime)
            .AddMinutes(attendance.Session.DurationMinutes);
        if (!request.IsAdmin && VietnamTime.UtcNow() - sessionEndUtc > TimeSpan.FromHours(24))
        {
            return Result.Failure<UpdateAttendanceResponse>(
                AttendanceErrors.UpdateWindowClosed(attendance.SessionId));
        }

        // Store old values for audit log
        var oldStatus = attendance.AttendanceStatus;
        var oldNote = attendance.Note;

        // Update attendance
        attendance.AttendanceStatus = request.AttendanceStatus;
        attendance.AbsenceType = request.AttendanceStatus == AttendanceStatus.Absent
            ? AbsenceType.NoNotice
            : null;

        if (request.Note is not null)
        {
            attendance.Note = request.Note;
        }

        // Create audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = userContext.UserId,
            Action = "UpdateAttendance",
            EntityType = "Attendance",
            EntityId = attendance.Id,
            DataBefore = JsonSerializer.Serialize(new { oldStatus = oldStatus.ToString(), oldNote }),
            DataAfter = JsonSerializer.Serialize(new { newStatus = request.AttendanceStatus.ToString(), newNote = request.Note }),
            IpAddress = userContext.IpAddress,
            CreatedAt = VietnamTime.UtcNow()
        };
        context.AuditLogs.Add(auditLog);

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateAttendanceResponse
        {
            Id = attendance.Id,
            SessionId = attendance.SessionId,
            StudentProfileId = attendance.StudentProfileId,
            AttendanceStatus = attendance.AttendanceStatus.ToString(),
            AbsenceType = attendance.AbsenceType.HasValue ? attendance.AbsenceType.Value.ToString() : null,
            Note = attendance.Note,
            UpdatedAt = attendance.MarkedAt
        };
    }
}

