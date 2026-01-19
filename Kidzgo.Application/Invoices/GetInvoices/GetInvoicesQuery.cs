using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Finance;

namespace Kidzgo.Application.Invoices.GetInvoices;

public sealed class GetInvoicesQuery : IQuery<GetInvoicesResponse>, IPageableQuery
{
    public Guid? BranchId { get; init; }
    public Guid? StudentProfileId { get; init; }
    public Guid? ClassId { get; init; }
    public InvoiceStatus? Status { get; init; }
    public InvoiceType? Type { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

