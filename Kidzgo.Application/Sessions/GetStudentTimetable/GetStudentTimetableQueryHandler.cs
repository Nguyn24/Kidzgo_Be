using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.GetStudentTimetable;

public sealed class GetStudentTimetableQueryHandler(
    IDbContext context
) : IQueryHandler<GetStudentTimetableQuery, GetStudentTimetableResponse>
{
    public async Task<Result<GetStudentTimetableResponse>> Handle(GetStudentTimetableQuery query, CancellationToken cancellationToken)
    {
        // Verify student exists
        var studentExists = await context.Profiles
            .AnyAsync(p => p.Id == query.StudentId && p.ProfileType == Domain.Users.ProfileType.Student, cancellationToken);

        if (!studentExists)
        {
            return Result.Failure<GetStudentTimetableResponse>(
                Error.NotFound("Student.NotFound", "Student not found"));
        }

        // Get sessions from classes where student is enrolled (Active status)
        // Note: When using Select projection, Include is not needed as EF Core will only load what's referenced
        var sessionsQuery = context.Sessions
            .Where(s => s.Class.ClassEnrollments
                .Any(ce => ce.StudentProfileId == query.StudentId && ce.Status == EnrollmentStatus.Active))
            .Where(s => s.Status != SessionStatus.Cancelled);

        // Filter by date range
        if (query.From.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(s => s.PlannedDatetime >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(s => s.PlannedDatetime <= query.To.Value);
        }

        var sessions = await sessionsQuery
            .OrderBy(s => s.PlannedDatetime)
            .Select(s => new TimetableItemDto
            {
                Id = s.Id,
                ClassId = s.ClassId,
                ClassCode = s.Class.Code,
                ClassTitle = s.Class.Title,
                PlannedDatetime = s.PlannedDatetime,
                ActualDatetime = s.ActualDatetime,
                DurationMinutes = s.DurationMinutes,
                ParticipationType = s.ParticipationType,
                Status = s.Status,
                PlannedRoomId = s.PlannedRoomId,
                PlannedRoomName = s.PlannedRoom != null ? s.PlannedRoom.Name : null,
                ActualRoomId = s.ActualRoomId,
                ActualRoomName = s.ActualRoom != null ? s.ActualRoom.Name : null,
                PlannedTeacherId = s.PlannedTeacherId,
                PlannedTeacherName = s.PlannedTeacher != null ? s.PlannedTeacher.Name : null,
                ActualTeacherId = s.ActualTeacherId,
                ActualTeacherName = s.ActualTeacher != null ? s.ActualTeacher.Name : null,
                PlannedAssistantId = s.PlannedAssistantId,
                PlannedAssistantName = s.PlannedAssistant != null ? s.PlannedAssistant.Name : null,
                LessonPlanId = s.LessonPlan != null ? s.LessonPlan.Id : null,
                LessonPlanLink = s.LessonPlan != null ? $"/api/lesson-plans/{s.LessonPlan.Id}" : null
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetStudentTimetableResponse
        {
            Sessions = sessions
        });
    }
}

