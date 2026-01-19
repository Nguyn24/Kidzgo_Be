using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Classes;

namespace Kidzgo.Domain.Finance;

public class Invoice : Entity
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid? ClassId { get; set; }
    public InvoiceType Type { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public DateOnly? DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public string? Description { get; set; }
    public string? PayosPaymentLink { get; set; }
    public string? PayosQr { get; set; }
    public long? PayosOrderCode { get; set; }
    public DateTime? IssuedAt { get; set; }
    public Guid? IssuedBy { get; set; }

    // Navigation properties
    public Branch Branch { get; set; } = null!;
    public Profile StudentProfile { get; set; } = null!;
    public Class? Class { get; set; }
    public User? IssuedByUser { get; set; }
    public ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
