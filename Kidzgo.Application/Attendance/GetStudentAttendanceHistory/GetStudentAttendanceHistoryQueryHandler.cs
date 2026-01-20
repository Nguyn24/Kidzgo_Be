using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Attendance.GetStudentAttendanceHistory;

public sealed class GetStudentAttendanceHistoryQueryHandler(IDbContext context)
    : IQueryHandler<GetStudentAttendanceHistoryQuery, Page<GetStudentAttendanceHistoryResponse>>
{
    public async Task<Result<Page<GetStudentAttendanceHistoryResponse>>> Handle(GetStudentAttendanceHistoryQuery request, CancellationToken cancellationToken)
    {
        var query = context.Attendances
            .Include(a => a.Session)
            .Where(a => a.StudentProfileId == request.StudentProfileId);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.Session.PlannedDatetime)
            .ApplyPagination(request.PageNumber, request.PageSize)
            .Select(a => new GetStudentAttendanceHistoryResponse
            {
                Id = a.Id,
                SessionId = a.SessionId,
                SessionDateTime = a.Session.PlannedDatetime,
                AttendanceStatus = a.AttendanceStatus.ToString(),
                AbsenceType = a.AbsenceType.HasValue ? a.AbsenceType.Value.ToString() : null,
                Note = a.Note
            })
            .ToListAsync(cancellationToken);

        return new Page<GetStudentAttendanceHistoryResponse>(items, total, request.PageNumber, request.PageSize);
    }
}

