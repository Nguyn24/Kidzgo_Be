using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MakeupCredits.SuggestSessions;

public sealed class SuggestMakeupSessionsQuery : IQuery<IEnumerable<SuggestedSessionResponse>>
{
    public Guid MakeupCreditId { get; set; }
    public int Limit { get; set; } = 5;
}

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
            .FirstOrDefaultAsync(s => s.Id == credit.SourceSessionId, cancellationToken);

        if (sourceSession is null)
        {
            return Result.Failure<IEnumerable<SuggestedSessionResponse>>(MakeupCreditErrors.NotFound(credit.SourceSessionId));
        }

        var now = DateTime.UtcNow;

        var suggestions = await context.Sessions
            .AsNoTracking()
            .Where(s => s.ClassId == sourceSession.ClassId)
            .Where(s => s.Status == SessionStatus.Scheduled && s.PlannedDatetime >= now)
            .OrderBy(s => s.PlannedDatetime)
            .Take(query.Limit)
            .Select(s => new SuggestedSessionResponse
            {
                SessionId = s.Id,
                ClassId = s.ClassId,
                PlannedDatetime = s.PlannedDatetime,
                BranchId = s.BranchId
            })
            .ToListAsync(cancellationToken);

        return suggestions;
    }
}

public sealed class SuggestedSessionResponse
{
    public Guid SessionId { get; set; }
    public Guid ClassId { get; set; }
    public Guid BranchId { get; set; }
    public DateTime PlannedDatetime { get; set; }
}

