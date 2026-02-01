using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Leads.GetLeads;

public sealed class GetLeadsQueryHandler(
    IDbContext context
) : IQueryHandler<GetLeadsQuery, GetLeadsResponse>
{
    public async Task<Result<GetLeadsResponse>> Handle(
        GetLeadsQuery query,
        CancellationToken cancellationToken)
    {
        var leadsQuery = context.Leads
            .Include(l => l.OwnerStaffUser)
            .Include(l => l.BranchPreferenceNavigation)
            .AsQueryable();

        // Apply filters
        if (query.Status.HasValue)
        {
            leadsQuery = leadsQuery.Where(l => l.Status == query.Status.Value);
        }

        if (query.Source.HasValue)
        {
            leadsQuery = leadsQuery.Where(l => l.Source == query.Source.Value);
        }

        if (query.OwnerStaffId.HasValue)
        {
            leadsQuery = leadsQuery.Where(l => l.OwnerStaffId == query.OwnerStaffId.Value);
        }

        if (query.BranchPreference.HasValue)
        {
            leadsQuery = leadsQuery.Where(l => l.BranchPreference == query.BranchPreference.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.Trim().ToLower();
            leadsQuery = leadsQuery.Where(l =>
                l.ContactName.ToLower().Contains(searchTerm) ||
                (l.Phone != null && l.Phone.Contains(searchTerm)) ||
                (l.Email != null && l.Email.ToLower().Contains(searchTerm)));
        }

        var totalCount = await leadsQuery.CountAsync(cancellationToken);

        var leads = await leadsQuery
            .OrderByDescending(l => l.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(l => new LeadDto
            {
                Id = l.Id,
                Source = l.Source.ToString(),
                Campaign = l.Campaign,
                ContactName = l.ContactName,
                ChildName = l.ChildName,
                ChildDateOfBirth = l.ChildDateOfBirth,
                Phone = l.Phone,
                ZaloId = l.ZaloId,
                Email = l.Email,
                Company = l.Company,
                Subject = l.Subject,
                BranchPreference = l.BranchPreference,
                BranchPreferenceName = l.BranchPreferenceNavigation != null
                    ? l.BranchPreferenceNavigation.Name
                    : null,
                ProgramInterest = l.ProgramInterest,
                Status = l.Status.ToString(),
                OwnerStaffId = l.OwnerStaffId,
                OwnerStaffName = l.OwnerStaffUser != null
                    ? l.OwnerStaffUser.Name ?? l.OwnerStaffUser.Email
                    : null,
                FirstResponseAt = l.FirstResponseAt,
                TouchCount = l.TouchCount,
                NextActionAt = l.NextActionAt,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetLeadsResponse
        {
            Leads = leads,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }
}

