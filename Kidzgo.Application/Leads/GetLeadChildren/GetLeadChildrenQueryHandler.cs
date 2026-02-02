using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Leads.GetLeadChildren;

public sealed class GetLeadChildrenQueryHandler(
    IDbContext context
) : IQueryHandler<GetLeadChildrenQuery, GetLeadChildrenResponse>
{
    public async Task<Result<GetLeadChildrenResponse>> Handle(
        GetLeadChildrenQuery query,
        CancellationToken cancellationToken)
    {
        // Validate Lead exists
        var leadExists = await context.Leads
            .AnyAsync(l => l.Id == query.LeadId, cancellationToken);

        if (!leadExists)
        {
            return Result.Failure<GetLeadChildrenResponse>(
                LeadErrors.NotFound(query.LeadId));
        }

        var children = await context.LeadChildren
            .Where(lc => lc.LeadId == query.LeadId)
            .OrderBy(lc => lc.CreatedAt)
            .Select(lc => new LeadChildDto
            {
                Id = lc.Id,
                LeadId = lc.LeadId,
                ChildName = lc.ChildName,
                Dob = lc.Dob,
                Gender = lc.Gender,
                ProgramInterest = lc.ProgramInterest,
                Notes = lc.Notes,
                Status = lc.Status.ToString(),
                ConvertedStudentProfileId = lc.ConvertedStudentProfileId,
                CreatedAt = lc.CreatedAt,
                UpdatedAt = lc.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetLeadChildrenResponse
        {
            Children = children
        });
    }
}

