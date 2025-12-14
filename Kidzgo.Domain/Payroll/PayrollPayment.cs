using Kidzgo.Domain.Common;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Payroll;

public class PayrollPayment : Entity
{
    public Guid Id { get; set; }
    public Guid PayrollRunId { get; set; }
    public Guid StaffUserId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTime? PaidAt { get; set; }
    public Guid? CashbookEntryId { get; set; }

    // Navigation properties
    public PayrollRun PayrollRun { get; set; } = null!;
    public User StaffUser { get; set; } = null!;
    public CashbookEntry? CashbookEntry { get; set; }
}
