using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Payroll;

namespace Kidzgo.Domain.Finance;

public class CashbookEntry : Entity
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public CashbookEntryType Type { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public string? Description { get; set; }
    public RelatedType? RelatedType { get; set; }
    public Guid? RelatedId { get; set; }
    public DateOnly EntryDate { get; set; }
    public Guid? CreatedBy { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Branch Branch { get; set; } = null!;
    public User? CreatedByUser { get; set; }
    public ICollection<PayrollPayment> PayrollPayments { get; set; } = new List<PayrollPayment>();
}
