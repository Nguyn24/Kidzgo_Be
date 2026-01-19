using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.GetSessionById;

public sealed class GetSessionByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetSessionByIdQuery, GetSessionByIdResponse>
{
    public async Task<Result<GetSessionByIdResponse>> Handle(GetSessionByIdQuery query, CancellationToken cancellationToken)
    {
        var session = await context.Sessions
            .Include(s => s.Class)
                .ThenInclude(c => c.Branch)
            .Include(s => s.Class)
                .ThenInclude(c => c.Program)
            .Include(s => s.Class)
                .ThenInclude(c => c.ClassEnrollments)
            .Include(s => s.PlannedRoom)
            .Include(s => s.ActualRoom)
            .Include(s => s.PlannedTeacher)
            .Include(s => s.ActualTeacher)
            .Include(s => s.PlannedAssistant)
            .Include(s => s.ActualAssistant)
            .Include(s => s.LessonPlan)
            .Include(s => s.Attendances)
                .ThenInclude(a => a.StudentProfile)
            .FirstOrDefaultAsync(s => s.Id == query.SessionId, cancellationToken);

        if (session == null)
        {
            return Result.Failure<GetSessionByIdResponse>(
                Error.NotFound("Session.NotFound", "Session not found"));
        }

        // Calculate attendance summary
        var totalStudents = session.Class.ClassEnrollments
            .Count(ce => ce.Status == EnrollmentStatus.Active);

        var attendances = session.Attendances.ToList();
        var presentCount = attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Present);
        var absentCount = attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Absent);
        var makeupCount = attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Makeup);
        var notMarkedCount = totalStudents - attendances.Count;

        var sessionDto = new SessionDetailDto
        {
            Id = session.Id,
            ClassId = session.ClassId,
            ClassCode = session.Class.Code,
            ClassTitle = session.Class.Title,
            BranchId = session.BranchId,
            BranchName = session.Branch.Name,
            PlannedDatetime = session.PlannedDatetime,
            ActualDatetime = session.ActualDatetime,
            DurationMinutes = session.DurationMinutes,
            ParticipationType = session.ParticipationType.ToString(),
            Status = session.Status.ToString(),
            PlannedRoomId = session.PlannedRoomId,
            PlannedRoomName = session.PlannedRoom != null ? session.PlannedRoom.Name : null,
            ActualRoomId = session.ActualRoomId,
            ActualRoomName = session.ActualRoom != null ? session.ActualRoom.Name : null,
            PlannedTeacherId = session.PlannedTeacherId,
            PlannedTeacherName = session.PlannedTeacher != null ? session.PlannedTeacher.Name : null,
            ActualTeacherId = session.ActualTeacherId,
            ActualTeacherName = session.ActualTeacher != null ? session.ActualTeacher.Name : null,
            PlannedAssistantId = session.PlannedAssistantId,
            PlannedAssistantName = session.PlannedAssistant != null ? session.PlannedAssistant.Name : null,
            ActualAssistantId = session.ActualAssistantId,
            ActualAssistantName = session.ActualAssistant != null ? session.ActualAssistant.Name : null,
            LessonPlanId = session.LessonPlan != null ? session.LessonPlan.Id : null,
            LessonPlanLink = session.LessonPlan != null ? $"/api/lesson-plans/{session.LessonPlan.Id}" : null,
            AttendanceSummary = new AttendanceSummaryDto
            {
                TotalStudents = totalStudents,
                PresentCount = presentCount,
                AbsentCount = absentCount,
                MakeupCount = makeupCount,
                NotMarkedCount = notMarkedCount
            }
        };

        return Result.Success(new GetSessionByIdResponse
        {
            Session = sessionDto
        });
    }
}

