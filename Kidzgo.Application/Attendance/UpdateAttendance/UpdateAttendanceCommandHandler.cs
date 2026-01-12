using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Attendance.UpdateAttendance;

public sealed class UpdateAttendanceCommandHandler(IDbContext context)
    : ICommandHandler<UpdateAttendanceCommand, UpdateAttendanceResponse>
{
    public async Task<Result<UpdateAttendanceResponse>> Handle(UpdateAttendanceCommand request, CancellationToken cancellationToken)
    {
        var attendance = await context.Attendances
            .Include(a => a.Session)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (attendance is null)
        {
            return Result.Failure<UpdateAttendanceResponse>(AttendanceErrors.NotFound(request.Id));
        }

        attendance.AttendanceStatus = request.AttendanceStatus;
        attendance.AbsenceType = request.AttendanceStatus == AttendanceStatus.Absent
            ? AbsenceType.NoNotice
            : null;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateAttendanceResponse
        {
            Id = attendance.Id,
            SessionId = attendance.SessionId,
            StudentProfileId = attendance.StudentProfileId,
            AttendanceStatus = attendance.AttendanceStatus,
            AbsenceType = attendance.AbsenceType
        };
    }
}

