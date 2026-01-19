using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Finance;

namespace Kidzgo.Application.Invoices.CreateInvoice;

public sealed class CreateInvoiceCommand : ICommand<CreateInvoiceResponse>
{
    public Guid BranchId { get; init; }
    public Guid StudentProfileId { get; init; }
    public Guid? ClassId { get; init; }
    public InvoiceType Type { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "VND";
    public DateOnly? DueDate { get; init; }
    public string? Description { get; init; }
    public List<CreateInvoiceLineDto>? InvoiceLines { get; init; }
}

public sealed class CreateInvoiceLineDto
{
    public InvoiceLineItemType ItemType { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string? Description { get; init; }
    public List<Guid>? SessionIds { get; init; }
}

