using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Schools;

namespace Kidzgo.Domain.Payroll;

public class Contract : Entity
{
    public Guid Id { get; set; }
    public Guid StaffUserId { get; set; }
    public ContractType ContractType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal? BaseSalary { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? AllowanceFixed { get; set; }
    public decimal? MinimumMonthlyHours { get; set; } // Minimum hours required per month to receive salary
    public decimal? OvertimeRateMultiplier { get; set; } // Multiplier for overtime (e.g., 1.5x, 2x)
    public Guid BranchId { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public User StaffUser { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public ICollection<ShiftAttendance> ShiftAttendances { get; set; } = new List<ShiftAttendance>();
}
