using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Attendance.GetSessionAttendance;

public sealed class GetSessionAttendanceQueryHandler(IDbContext context)
    : IQueryHandler<GetSessionAttendanceQuery, List<GetSessionAttendanceResponse>>
{
    public async Task<Result<List<GetSessionAttendanceResponse>>> Handle(GetSessionAttendanceQuery request, CancellationToken cancellationToken)
    {
        var data = await context.Attendances
            .Include(a => a.StudentProfile)
            .Where(a => a.SessionId == request.SessionId)
            .Select(a => new GetSessionAttendanceResponse
            {
                Id = a.Id,
                StudentProfileId = a.StudentProfileId,
                StudentName = a.StudentProfile.DisplayName,
                AttendanceStatus = a.AttendanceStatus.ToString(),
                AbsenceType = a.AbsenceType.HasValue ? a.AbsenceType.Value.ToString() : null,
                HasMakeupCredit = context.MakeupCredits.Any(c =>
                    c.StudentProfileId == a.StudentProfileId &&
                    c.Status == Kidzgo.Domain.Sessions.MakeupCreditStatus.Available)
            })
            .ToListAsync(cancellationToken);

        return data;
    }
}

