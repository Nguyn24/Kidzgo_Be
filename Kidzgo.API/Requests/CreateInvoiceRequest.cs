using Kidzgo.Domain.Finance;

namespace Kidzgo.API.Requests;

public sealed class CreateInvoiceRequest
{
    public Guid BranchId { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid? ClassId { get; set; }
    public InvoiceType Type { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public DateOnly? DueDate { get; set; }
    public string? Description { get; set; }
    public List<CreateInvoiceLineRequest>? InvoiceLines { get; set; }
}

public sealed class CreateInvoiceLineRequest
{
    public InvoiceLineItemType ItemType { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Description { get; set; }
    public List<Guid>? SessionIds { get; set; }
}

