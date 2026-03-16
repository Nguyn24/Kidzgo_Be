using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Attendance.GetSessionAttendance;

public sealed class GetSessionAttendanceQueryHandler(IDbContext context)
    : IQueryHandler<GetSessionAttendanceQuery, GetSessionAttendanceListResponse>
{
    public async Task<Result<GetSessionAttendanceListResponse>> Handle(GetSessionAttendanceQuery request, CancellationToken cancellationToken)
    {
        // Get session with class info
        var session = await context.Sessions
            .AsNoTracking()
            .Include(s => s.Class)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure<GetSessionAttendanceListResponse>(SessionErrors.NotFound(request.SessionId));
        }

        // Get enrollments and attendance data
        var enrollmentsQuery = context.ClassEnrollments
            .AsNoTracking()
            .Include(ce => ce.StudentProfile)
            .Where(ce => ce.ClassId == session.ClassId && ce.Status == EnrollmentStatus.Active);

        var attendancesQuery = context.Attendances
            .AsNoTracking()
            .Where(a => a.SessionId == request.SessionId);

        var data = await enrollmentsQuery
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
                Note = x.Attendance != null ? x.Attendance.Note : null,
                MarkedAt = x.Attendance != null ? x.Attendance.MarkedAt : null
            })
            .ToListAsync(cancellationToken);

        // Calculate summary
        var summary = new AttendanceSummary
        {
            TotalStudents = data.Count,
            PresentCount = data.Count(a => a.AttendanceStatus == "Present"),
            AbsentCount = data.Count(a => a.AttendanceStatus == "Absent"),
            MakeupCount = data.Count(a => a.AttendanceStatus == "Makeup"),
            NotMarkedCount = data.Count(a => a.AttendanceStatus == "NotMarked")
        };

        var response = new GetSessionAttendanceListResponse
        {
            SessionId = session.Id,
            SessionName = session.Class?.Title,
            Date = DateOnly.FromDateTime(session.PlannedDatetime),
            StartTime = TimeOnly.FromDateTime(session.PlannedDatetime),
            EndTime = TimeOnly.FromDateTime(session.PlannedDatetime.AddMinutes(session.DurationMinutes)),
            Summary = summary,
            Attendances = data
        };

        return response;
    }
}

