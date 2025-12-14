using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Payroll;

public class ShiftAttendance : Entity
{
    public Guid Id { get; set; }
    public Guid StaffUserId { get; set; }
    public Guid? ContractId { get; set; }
    public DateOnly ShiftDate { get; set; }
    public decimal ShiftHours { get; set; }
    public string? Role { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Navigation properties
    public User StaffUser { get; set; } = null!;
    public Contract? Contract { get; set; }
    public User? ApprovedByUser { get; set; }
}
