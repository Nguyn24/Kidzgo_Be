using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MakeupCredits.GetAllMakeupCredits;

public sealed class GetAllMakeupCreditsQueryHandler(IDbContext context)
    : IQueryHandler<GetAllMakeupCreditsQuery, Page<MakeupCreditResponse>>
{
    public async Task<Result<Page<MakeupCreditResponse>>> Handle(
        GetAllMakeupCreditsQuery query,
        CancellationToken cancellationToken)
    {
        var creditsQuery = context.MakeupCredits
            .AsNoTracking()
            .AsQueryable();

        if (query.StudentProfileId.HasValue)
        {
            creditsQuery = creditsQuery.Where(c => c.StudentProfileId == query.StudentProfileId.Value);
        }

        if (query.Status.HasValue)
        {
            creditsQuery = creditsQuery.Where(c => c.Status == query.Status.Value);
        }

        if (query.CreatedReason.HasValue)
        {
            creditsQuery = creditsQuery.Where(c => c.CreatedReason == query.CreatedReason.Value);
        }

        if (query.CreatedFrom.HasValue)
        {
            creditsQuery = creditsQuery.Where(c => c.CreatedAt >= query.CreatedFrom.Value);
        }

        if (query.CreatedTo.HasValue)
        {
            creditsQuery = creditsQuery.Where(c => c.CreatedAt <= query.CreatedTo.Value);
        }

        if (query.ExpiresFrom.HasValue)
        {
            creditsQuery = creditsQuery.Where(c => c.ExpiresAt.HasValue && c.ExpiresAt.Value >= query.ExpiresFrom.Value);
        }

        if (query.ExpiresTo.HasValue)
        {
            creditsQuery = creditsQuery.Where(c => c.ExpiresAt.HasValue && c.ExpiresAt.Value <= query.ExpiresTo.Value);
        }

        // Apply branch filter (through student's class enrollments)
        if (query.BranchId.HasValue)
        {
            creditsQuery = creditsQuery.Where(c => 
                c.StudentProfile.ClassEnrollments.Any(ce => ce.Class.BranchId == query.BranchId.Value));
        }

        var totalCount = await creditsQuery.CountAsync(cancellationToken);

        var credits = await creditsQuery
            .OrderByDescending(c => c.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(c => new MakeupCreditResponse
            {
                Id = c.Id,
                StudentProfileId = c.StudentProfileId,
                SourceSessionId = c.SourceSessionId,
                Status = c.Status.ToString(),
                CreatedReason = c.CreatedReason.ToString(),
                ExpiresAt = c.ExpiresAt,
                UsedSessionId = c.UsedSessionId,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new Page<MakeupCreditResponse>(
            credits,
            totalCount,
            query.PageNumber,
            query.PageSize);
    }
}