using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Finance;

public class Payment : Entity
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? ReferenceCode { get; set; }
    public Guid? ConfirmedBy { get; set; }
    public string? EvidenceUrl { get; set; }

    // Navigation properties
    public Invoice Invoice { get; set; } = null!;
    public User? ConfirmedByUser { get; set; }
}
