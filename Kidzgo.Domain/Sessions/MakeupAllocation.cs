using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Sessions;

public class MakeupAllocation : Entity
{
    public Guid Id { get; set; }
    public Guid MakeupCreditId { get; set; }
    public Guid TargetSessionId { get; set; }
    public Guid? AssignedBy { get; set; }
    public DateTime? AssignedAt { get; set; }

    // Navigation properties
    public MakeupCredit MakeupCredit { get; set; } = null!;
    public Session TargetSession { get; set; } = null!;
    public User? AssignedByUser { get; set; }
}
