using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Leads.GetLeadSLA;

public sealed class GetLeadSLAQueryHandler(
    IDbContext context
) : IQueryHandler<GetLeadSLAQuery, GetLeadSLAResponse>
{
    public async Task<Result<GetLeadSLAResponse>> Handle(
        GetLeadSLAQuery query,
        CancellationToken cancellationToken)
    {
        var lead = await context.Leads
            .FirstOrDefaultAsync(l => l.Id == query.LeadId, cancellationToken);

        if (lead is null)
        {
            return Result.Failure<GetLeadSLAResponse>(
                LeadErrors.NotFound(query.LeadId));
        }

        // UC-024: Calculate SLA metrics
        const int slaTargetHours = 24;
        var timeToFirstResponse = lead.FirstResponseAt.HasValue
            ? lead.FirstResponseAt.Value - lead.CreatedAt
            : (TimeSpan?)null;

        var isSLACompliant = timeToFirstResponse.HasValue
            ? timeToFirstResponse.Value.TotalHours <= slaTargetHours
            : (bool?)null;

        var isSLAOverdue = !lead.FirstResponseAt.HasValue &&
            (DateTime.UtcNow - lead.CreatedAt).TotalHours > slaTargetHours;

        return Result.Success(new GetLeadSLAResponse
        {
            LeadId = lead.Id,
            CreatedAt = lead.CreatedAt,
            FirstResponseAt = lead.FirstResponseAt,
            TimeToFirstResponse = timeToFirstResponse,
            SLATargetHours = slaTargetHours,
            IsSLACompliant = isSLACompliant,
            IsSLAOverdue = isSLAOverdue
        });
    }
}

