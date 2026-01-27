using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.CRM;

namespace Kidzgo.Application.Leads.GetLeads;

public sealed class GetLeadsQuery : IQuery<GetLeadsResponse>
{
    public LeadStatus? Status { get; init; }
    public LeadSource? Source { get; init; }
    public Guid? OwnerStaffId { get; init; }
    public Guid? BranchPreference { get; init; }
    public string? SearchTerm { get; init; } // Search by name, phone, email
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

