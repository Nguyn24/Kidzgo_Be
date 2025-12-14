using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Payroll;

public class PayrollLine : Entity
{
    public Guid Id { get; set; }
    public Guid PayrollRunId { get; set; }
    public Guid StaffUserId { get; set; }
    public PayrollComponentType ComponentType { get; set; }
    public Guid? SourceId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }

    // Navigation properties
    public PayrollRun PayrollRun { get; set; } = null!;
    public User StaffUser { get; set; } = null!;
}
