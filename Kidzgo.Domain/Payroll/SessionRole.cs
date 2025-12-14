using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Domain.Payroll;

public class SessionRole : Entity
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid StaffUserId { get; set; }
    public SessionRoleType RoleType { get; set; }
    public decimal? PayableUnitPrice { get; set; }
    public decimal? PayableAllowance { get; set; }

    // Navigation properties
    public Session Session { get; set; } = null!;
    public User StaffUser { get; set; } = null!;
}
