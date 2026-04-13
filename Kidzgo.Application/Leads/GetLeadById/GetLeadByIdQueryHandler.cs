using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Leads.GetLeadById;

public sealed class GetLeadByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetLeadByIdQuery, GetLeadByIdResponse>
{
    public async Task<Result<GetLeadByIdResponse>> Handle(
        GetLeadByIdQuery query,
        CancellationToken cancellationToken)
    {
        var lead = await context.Leads
            .Include(l => l.OwnerStaffUser)
            .Include(l => l.BranchPreferenceNavigation)
            .Include(l => l.LeadChildren)
            .FirstOrDefaultAsync(l => l.Id == query.LeadId, cancellationToken);

        if (lead is null)
        {
            return Result.Failure<GetLeadByIdResponse>(
                LeadErrors.NotFound(query.LeadId));
        }

        var programInterestSummary = lead.LeadChildren
            .Select(child => child.ProgramInterest?.Trim())
            .Where(programInterest => !string.IsNullOrWhiteSpace(programInterest))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Result.Success(new GetLeadByIdResponse
        {
            Id = lead.Id,
            Source = lead.Source.ToString(),
            Campaign = lead.Campaign,
            ContactName = lead.ContactName,
            Phone = lead.Phone,
            ZaloId = lead.ZaloId,
            Email = lead.Email,
            Company = lead.Company,
            Subject = lead.Subject,
            ProgramInterestSummary = programInterestSummary.Count > 0
                ? string.Join(", ", programInterestSummary)
                : null,
            BranchPreference = lead.BranchPreference,
            BranchPreferenceName = lead.BranchPreferenceNavigation?.Name,
            Notes = lead.Notes,
            Status = lead.Status.ToString(),
            OwnerStaffId = lead.OwnerStaffId,
            OwnerStaffName = lead.OwnerStaffUser != null
                ? lead.OwnerStaffUser.Name ?? lead.OwnerStaffUser.Email
                : null,
            FirstResponseAt = lead.FirstResponseAt,
            TouchCount = lead.TouchCount,
            NextActionAt = lead.NextActionAt,
            CreatedAt = lead.CreatedAt,
            UpdatedAt = lead.UpdatedAt
        });
    }
}

