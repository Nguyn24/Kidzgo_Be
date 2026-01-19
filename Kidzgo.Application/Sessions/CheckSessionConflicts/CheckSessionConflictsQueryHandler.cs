using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.CheckSessionConflicts;

public sealed class CheckSessionConflictsQueryHandler(
    IDbContext context,
    SessionConflictChecker conflictChecker
) : IQueryHandler<CheckSessionConflictsQuery, CheckSessionConflictsResponse>
{
    public async Task<Result<CheckSessionConflictsResponse>> Handle(
        CheckSessionConflictsQuery query,
        CancellationToken cancellationToken)
    {
        var plannedUtc = query.PlannedDatetime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(query.PlannedDatetime, DateTimeKind.Utc)
            : query.PlannedDatetime.ToUniversalTime();

        var sessionId = query.SessionId ?? Guid.Empty;

         // Check conflicts
         var conflictResult = await conflictChecker.CheckConflictsAsync(
             sessionId,
             plannedUtc,
             query.DurationMinutes,
             query.PlannedRoomId,
             query.PlannedTeacherId,
             query.PlannedAssistantId,
             cancellationToken);

         // Get suggestions nếu có conflicts
         ConflictSuggestionsDto? suggestions = null;
         if (conflictResult.HasConflicts)
         {
             // Cần branchId để get suggestions
             Guid? branchId = null;
             if (query.SessionId.HasValue)
             {
                 var session = await context.Sessions
                     .FirstOrDefaultAsync(s => s.Id == query.SessionId.Value, cancellationToken);
                 branchId = session?.BranchId;
             }
             else if (query.PlannedRoomId.HasValue)
             {
                 var room = await context.Classrooms
                     .FirstOrDefaultAsync(r => r.Id == query.PlannedRoomId.Value, cancellationToken);
                 branchId = room?.BranchId;
             }

             if (branchId.HasValue)
             {
                 var suggestionsResult = await conflictChecker.GetSuggestionsAsync(
                     branchId.Value,
                     plannedUtc,
                     query.DurationMinutes,
                     query.PlannedRoomId,
                     query.PlannedTeacherId,
                     cancellationToken);

                 suggestions = new ConflictSuggestionsDto
                 {
                     SuggestedRooms = suggestionsResult.SuggestedRooms.Select(r => new SuggestedRoomDto
                     {
                         Id = r.Id,
                         Name = r.Name,
                         Capacity = r.Capacity
                     }).ToList(),
                     AlternativeSlots = suggestionsResult.AlternativeSlots
                 };
             }
         }

         return Result.Success(new CheckSessionConflictsResponse
         {
             HasConflicts = conflictResult.HasConflicts,
             Conflicts = conflictResult.Conflicts.Select(c => new ConflictDto
             {
                 Type = c.Type.ToString(),
                 SessionId = c.SessionId,
                 ClassId = c.ClassId,
                 ClassCode = c.ClassCode,
                 ClassTitle = c.ClassTitle,
                 ConflictDatetime = c.ConflictDatetime,
                 DurationMinutes = c.DurationMinutes,
                 RoomId = c.RoomId,
                 RoomName = c.RoomName
             }).ToList(),
             Suggestions = suggestions
         });
         
    }
}

