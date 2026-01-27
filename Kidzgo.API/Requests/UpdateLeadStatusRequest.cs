using Kidzgo.Domain.CRM;

namespace Kidzgo.API.Requests;

public sealed class UpdateLeadStatusRequest
{
    public LeadStatus Status { get; set; }
}

