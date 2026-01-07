using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.GetTeacherTimetable;

public sealed class GetTeacherTimetableQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetTeacherTimetableQuery, GetTeacherTimetableResponse>
{
    public async Task<Result<GetTeacherTimetableResponse>> Handle(GetTeacherTimetableQuery query, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        // Get sessions where teacher is PlannedTeacher or ActualTeacher
        // Note: When using Select projection, Include is not needed as EF Core will only load what's referenced
        var sessionsQuery = context.Sessions
            .Where(s => (s.PlannedTeacherId == userId || s.ActualTeacherId == userId) 
                     && s.Status != SessionStatus.Cancelled);

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

        return Result.Success(new GetTeacherTimetableResponse
        {
            Sessions = sessions
        });
    }
}

