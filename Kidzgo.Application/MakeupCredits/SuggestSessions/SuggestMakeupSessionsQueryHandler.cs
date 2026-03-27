using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MakeupCredits.SuggestSessions;

public sealed class SuggestMakeupSessionsQueryHandler(IDbContext context)
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

        var studentSessionTimes = await context.Sessions
            .AsNoTracking()
            .Where(s => s.Class.ClassEnrollments
                .Any(ce => ce.StudentProfileId == credit.StudentProfileId &&
                           ce.Status == EnrollmentStatus.Active))
            .Where(s => s.Status == SessionStatus.Scheduled)
            .Select(s => new
            {
                Start = s.PlannedDatetime,
                End = s.PlannedDatetime.AddMinutes(s.DurationMinutes)
            })
            .ToListAsync(cancellationToken);

        var fromDate = query.FromDate ?? DateOnly.FromDateTime(now);
        var toDate = query.ToDate ?? fromDate.AddDays(7);
        string? timeOfDay = query.TimeOfDay?.ToLower().Trim();

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

        var classIds = rawSuggestions
            .Select(s => s.ClassId)
            .Distinct()
            .ToList();

        var sessionIds = rawSuggestions
            .Select(s => s.Id)
            .ToList();

        var enrollmentCounts = await context.ClassEnrollments
            .AsNoTracking()
            .Where(e => classIds.Contains(e.ClassId) && e.Status == EnrollmentStatus.Active)
            .GroupBy(e => e.ClassId)
            .Select(g => new { ClassId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClassId, x => x.Count, cancellationToken);

        var allocationCounts = await context.MakeupAllocations
            .AsNoTracking()
            .Where(a => sessionIds.Contains(a.TargetSessionId) &&
                        a.MakeupCreditId != credit.Id &&
                        a.Status != MakeupAllocationStatus.Cancelled)
            .GroupBy(a => a.TargetSessionId)
            .Select(g => new { SessionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SessionId, x => x.Count, cancellationToken);

        var minGap = TimeSpan.FromHours(2);

        var filtered = rawSuggestions
            .Where(s =>
            {
                var activeEnrollmentCount = enrollmentCounts.GetValueOrDefault(s.ClassId);
                var activeAllocationCount = allocationCounts.GetValueOrDefault(s.Id);
                if (activeEnrollmentCount + activeAllocationCount >= s.Class.Capacity)
                {
                    return false;
                }

                var start = s.PlannedDatetime;
                var end = s.PlannedDatetime.AddMinutes(s.DurationMinutes);

                return !studentSessionTimes.Any(st =>
                {
                    var overlap = start < st.End && end > st.Start;
                    var gapToStart = (start - st.End).Duration();
                    var gapToEnd = (st.Start - end).Duration();
                    var tooClose = gapToStart < minGap || gapToEnd < minGap;

                    return overlap || tooClose;
                });
            })
            .Select(s => new SuggestedSessionResponse
            {
                SessionId = s.Id,
                ClassId = s.ClassId,
                ClassCode = s.Class.Code,
                ClassTitle = s.Class.Title,
                ProgramName = s.Class.Program.Name,
                ProgramCode = s.Class.Program.Code,
                PlannedDatetime = s.PlannedDatetime,
                PlannedEndDatetime = s.PlannedDatetime.AddMinutes(s.DurationMinutes),
                BranchId = s.BranchId
            })
            .ToList();

        return filtered;
    }
}
