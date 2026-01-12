using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.GetSessions;

public sealed class GetSessionsQueryHandler(
    IDbContext context
) : IQueryHandler<GetSessionsQuery, GetSessionsResponse>
{
    public async Task<Result<GetSessionsResponse>> Handle(GetSessionsQuery query, CancellationToken cancellationToken)
    {
        var sessionsQuery = context.Sessions
            .Include(s => s.Class)
                .ThenInclude(c => c.Branch)
            .Include(s => s.PlannedRoom)
            .Include(s => s.ActualRoom)
            .Include(s => s.PlannedTeacher)
            .Include(s => s.ActualTeacher)
            .Include(s => s.PlannedAssistant)
            .Include(s => s.ActualAssistant)
            .AsQueryable();

        if (query.ClassId.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(s => s.ClassId == query.ClassId.Value);
        }

        if (query.BranchId.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(s => s.BranchId == query.BranchId.Value);
        }

        if (query.Status.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(s => s.Status == query.Status.Value);
        }

        if (query.From.HasValue)
        {
            var fromUtc = query.From.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(query.From.Value, DateTimeKind.Utc)
                : query.From.Value.ToUniversalTime();
            sessionsQuery = sessionsQuery.Where(s => s.PlannedDatetime >= fromUtc);
        }

        if (query.To.HasValue)
        {
            var toUtc = query.To.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(query.To.Value, DateTimeKind.Utc)
                : query.To.Value.ToUniversalTime();
            toUtc = toUtc.Date.AddDays(1).AddTicks(-1);
            sessionsQuery = sessionsQuery.Where(s => s.PlannedDatetime <= toUtc);
        }

        var totalCount = await sessionsQuery.CountAsync(cancellationToken);

        var items = await sessionsQuery
            .OrderBy(s => s.PlannedDatetime)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(s => new SessionListItemDto
            {
                Id = s.Id,
                ClassId = s.ClassId,
                ClassCode = s.Class.Code,
                ClassTitle = s.Class.Title,
                BranchId = s.BranchId,
                BranchName = s.Branch.Name,
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
                ActualAssistantId = s.ActualAssistantId,
                ActualAssistantName = s.ActualAssistant != null ? s.ActualAssistant.Name : null
            })
            .ToListAsync(cancellationToken);

        var page = new Page<SessionListItemDto>(
            items,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetSessionsResponse
        {
            Sessions = page
        };
    }
}


