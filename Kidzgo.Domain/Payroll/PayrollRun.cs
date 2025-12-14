using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Payroll;

public class PayrollRun : Entity
{
    public Guid Id { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public Guid BranchId { get; set; }
    public PayrollRunStatus Status { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Branch Branch { get; set; } = null!;
    public User? ApprovedByUser { get; set; }
    public ICollection<PayrollLine> PayrollLines { get; set; } = new List<PayrollLine>();
    public ICollection<PayrollPayment> PayrollPayments { get; set; } = new List<PayrollPayment>();
}
