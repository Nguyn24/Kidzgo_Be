using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Schools;

namespace Kidzgo.Domain.Payroll;

public class MonthlyWorkHours : Entity
{
    public Guid Id { get; set; }
    public Guid StaffUserId { get; set; }
    public Guid ContractId { get; set; }
    public Guid BranchId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; } // 1-12
    public decimal TotalHours { get; set; } // Total hours worked in the month
    public decimal TeachingHours { get; set; } // Hours from teaching sessions (for teachers)
    public decimal RegularHours { get; set; } // Regular hours (from shift attendance)
    public decimal OvertimeHours { get; set; } // Overtime hours (hours exceeding minimum)
    public int TeachingSessions { get; set; } // Number of sessions taught (for teachers)
    public bool IsLocked { get; set; } // Whether this month's hours are locked (for payroll calculation)

    // Navigation properties
    public User StaffUser { get; set; } = null!;
    public Contract Contract { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}

