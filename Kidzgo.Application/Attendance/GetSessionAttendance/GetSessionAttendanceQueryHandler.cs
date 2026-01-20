using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Attendance.GetSessionAttendance;

public sealed class GetSessionAttendanceQueryHandler(IDbContext context)
    : IQueryHandler<GetSessionAttendanceQuery, List<GetSessionAttendanceResponse>>
{
    public async Task<Result<List<GetSessionAttendanceResponse>>> Handle(GetSessionAttendanceQuery request, CancellationToken cancellationToken)
    {
        var session = await context.Sessions
            .AsNoTracking()
            .Select(s => new { s.Id, s.ClassId })
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure<List<GetSessionAttendanceResponse>>(SessionErrors.NotFound(request.SessionId));
        }

        var attendancesQuery = context.Attendances
            .AsNoTracking()
            .Where(a => a.SessionId == request.SessionId);

        var data = await context.ClassEnrollments
            .AsNoTracking()
            .Include(ce => ce.StudentProfile)
            .Where(ce => ce.ClassId == session.ClassId && ce.Status == EnrollmentStatus.Active)
            .GroupJoin(
                attendancesQuery,
                ce => ce.StudentProfileId,
                a => a.StudentProfileId,
                (ce, attendances) => new { Enrollment = ce, Attendance = attendances.FirstOrDefault() })
            .Select(x => new GetSessionAttendanceResponse
            {
                Id = x.Attendance != null ? x.Attendance.Id : Guid.Empty,
                StudentProfileId = x.Enrollment.StudentProfileId,
                StudentName = x.Enrollment.StudentProfile.DisplayName,
                AttendanceStatus = x.Attendance != null ? x.Attendance.AttendanceStatus.ToString() : "NotMarked",
                AbsenceType = x.Attendance != null && x.Attendance.AbsenceType.HasValue
                    ? x.Attendance.AbsenceType.Value.ToString()
                    : null,
                HasMakeupCredit = context.MakeupCredits.Any(c =>
                    c.StudentProfileId == x.Enrollment.StudentProfileId &&
                    c.Status == MakeupCreditStatus.Available),
                Note = x.Attendance != null ? x.Attendance.Note : null
            })
            .ToListAsync(cancellationToken);

        return data;
    }
}

