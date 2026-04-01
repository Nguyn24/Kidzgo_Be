using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MakeupCredits.SuggestSessions;

public sealed class SuggestMakeupSessionsQueryHandler(
    IDbContext context,
    SessionParticipantService sessionParticipantService)
    : IQueryHandler<SuggestMakeupSessionsQuery, IEnumerable<SuggestedSessionResponse>>
{
    public async Task<Result<IEnumerable<SuggestedSessionResponse>>> Handle(
        SuggestMakeupSessionsQuery query,
        CancellationToken cancellationToken)
    {
        var credit = await context.MakeupCredits
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.MakeupCreditId, cancellationToken);

        if (credit is null)
        {
            return Result.Failure<IEnumerable<SuggestedSessionResponse>>(MakeupCreditErrors.NotFound(query.MakeupCreditId));
        }

        var sourceSession = await context.Sessions
            .AsNoTracking()
            .Include(s => s.Class)
            .ThenInclude(c => c.Program)
            .FirstOrDefaultAsync(s => s.Id == credit.SourceSessionId, cancellationToken);

        if (sourceSession is null)
        {
            return Result.Failure<IEnumerable<SuggestedSessionResponse>>(MakeupCreditErrors.NotFound(credit.SourceSessionId));
        }

        var now = DateTime.UtcNow;

        Session? currentAllocatedSession = null;
        Guid? restrictedProgramId = null;
        if (credit.Status == MakeupCreditStatus.Used && credit.UsedSessionId.HasValue)
        {
            currentAllocatedSession = await context.Sessions
                .AsNoTracking()
                .Include(s => s.Class)
                .ThenInclude(c => c.Program)
                .FirstOrDefaultAsync(s => s.Id == credit.UsedSessionId.Value, cancellationToken);

            if (currentAllocatedSession != null &&
                DateOnly.FromDateTime(currentAllocatedSession.PlannedDatetime) > DateOnly.FromDateTime(now))
            {
                restrictedProgramId = currentAllocatedSession.Class.ProgramId;
            }
        }

        var fromDate = query.FromDate ?? DateOnly.FromDateTime(now);
        var toDate = query.ToDate ?? fromDate.AddDays(7);
        string? timeOfDay = query.TimeOfDay?.ToLower().Trim();

        var studentSessionTimes = await sessionParticipantService.GetStudentBookedSlotsAsync(
            credit.StudentProfileId,
            fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            toDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc),
            cancellationToken);

        var rawSuggestionsQuery = context.Sessions
            .AsNoTracking()
            .Include(s => s.Class)
            .ThenInclude(c => c.Program)
            .Where(s => s.Id != sourceSession.Id)
            .Where(s => !credit.UsedSessionId.HasValue || s.Id != credit.UsedSessionId.Value)
            .Where(s => s.Status == SessionStatus.Scheduled && s.PlannedDatetime >= now)
            .Where(s => DateOnly.FromDateTime(s.PlannedDatetime) >= fromDate)
            .Where(s => DateOnly.FromDateTime(s.PlannedDatetime) <= toDate)
            .Where(s => DateOnly.FromDateTime(s.PlannedDatetime).DayOfWeek == DayOfWeek.Saturday ||
                        DateOnly.FromDateTime(s.PlannedDatetime).DayOfWeek == DayOfWeek.Sunday)
            .Where(s => s.BranchId == sourceSession.BranchId)
            .Where(s => s.ClassId != sourceSession.ClassId);

        if (restrictedProgramId.HasValue)
        {
            rawSuggestionsQuery = rawSuggestionsQuery.Where(s => s.Class.ProgramId == restrictedProgramId.Value);
        }

        if (!string.IsNullOrWhiteSpace(timeOfDay))
        {
            rawSuggestionsQuery = timeOfDay switch
            {
                "morning" => rawSuggestionsQuery.Where(s => s.PlannedDatetime.TimeOfDay >= new TimeSpan(6, 0, 0) &&
                                                            s.PlannedDatetime.TimeOfDay < new TimeSpan(12, 0, 0)),
                "afternoon" => rawSuggestionsQuery.Where(s => s.PlannedDatetime.TimeOfDay >= new TimeSpan(12, 0, 0) &&
                                                              s.PlannedDatetime.TimeOfDay < new TimeSpan(18, 0, 0)),
                "evening" => rawSuggestionsQuery.Where(s => s.PlannedDatetime.TimeOfDay >= new TimeSpan(18, 0, 0) &&
                                                            s.PlannedDatetime.TimeOfDay < new TimeSpan(23, 0, 0)),
                _ => rawSuggestionsQuery
            };
        }

        var rawSuggestions = await rawSuggestionsQuery
            .OrderBy(s => s.PlannedDatetime)
            .ToListAsync(cancellationToken);

        if (!rawSuggestions.Any())
        {
            return Array.Empty<SuggestedSessionResponse>();
        }

        var minGap = TimeSpan.FromHours(2);

        var filtered = new List<SuggestedSessionResponse>();
        foreach (var suggestion in rawSuggestions)
        {
            var participantCount = (await sessionParticipantService
                .GetParticipantsAsync(suggestion.Id, cancellationToken))
                .Count;

            if (participantCount >= suggestion.Class.Capacity)
            {
                continue;
            }

            var start = suggestion.PlannedDatetime;
            var end = suggestion.PlannedDatetime.AddMinutes(suggestion.DurationMinutes);
            var hasConflict = studentSessionTimes.Any(st =>
            {
                var overlap = start < st.End && end > st.Start;
                var gapToStart = (start - st.End).Duration();
                var gapToEnd = (st.Start - end).Duration();
                var tooClose = gapToStart < minGap || gapToEnd < minGap;

                return overlap || tooClose;
            });

            if (hasConflict)
            {
                continue;
            }

            filtered.Add(new SuggestedSessionResponse
            {
                SessionId = suggestion.Id,
                ClassId = suggestion.ClassId,
                ClassCode = suggestion.Class.Code,
                ClassTitle = suggestion.Class.Title,
                ProgramName = suggestion.Class.Program.Name,
                ProgramCode = suggestion.Class.Program.Code,
                PlannedDatetime = suggestion.PlannedDatetime,
                PlannedEndDatetime = suggestion.PlannedDatetime.AddMinutes(suggestion.DurationMinutes),
                BranchId = suggestion.BranchId
            });
        }

        return filtered;
    }
}
