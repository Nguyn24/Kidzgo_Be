using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Finance;

public class InvoiceLine : Entity
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public InvoiceLineItemType ItemType { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Description { get; set; }
    public string? SessionIds { get; set; }

    // Navigation properties
    public Invoice Invoice { get; set; } = null!;
}
