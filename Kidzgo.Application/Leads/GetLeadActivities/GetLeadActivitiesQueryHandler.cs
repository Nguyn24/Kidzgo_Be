using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Leads.GetLeadActivities;

public sealed class GetLeadActivitiesQueryHandler(
    IDbContext context
) : IQueryHandler<GetLeadActivitiesQuery, GetLeadActivitiesResponse>
{
    public async Task<Result<GetLeadActivitiesResponse>> Handle(
        GetLeadActivitiesQuery query,
        CancellationToken cancellationToken)
    {
        // Verify lead exists
        var leadExists = await context.Leads
            .AnyAsync(l => l.Id == query.LeadId, cancellationToken);

        if (!leadExists)
        {
            return Result.Failure<GetLeadActivitiesResponse>(
                LeadErrors.NotFound(query.LeadId));
        }

        var activities = await context.LeadActivities
            .Include(a => a.CreatedByUser)
            .Where(a => a.LeadId == query.LeadId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new LeadActivityDto
            {
                Id = a.Id,
                ActivityType = a.ActivityType.ToString(),
                Content = a.Content,
                NextActionAt = a.NextActionAt,
                CreatedBy = a.CreatedBy,
                CreatedByName = a.CreatedByUser != null
                    ? a.CreatedByUser.Name ?? a.CreatedByUser.Email
                    : null,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetLeadActivitiesResponse
        {
            LeadId = query.LeadId,
            Activities = activities
        });
    }
}

