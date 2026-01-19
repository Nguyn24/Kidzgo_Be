using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Finance;

namespace Kidzgo.Application.Invoices.GetParentInvoices;

public sealed class GetParentInvoicesQuery : IQuery<GetParentInvoicesResponse>, IPageableQuery
{
    public Guid ParentProfileId { get; init; }
    public InvoiceStatus? Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

