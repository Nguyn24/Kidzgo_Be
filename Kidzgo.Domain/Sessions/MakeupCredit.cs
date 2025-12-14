using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Sessions;

public class MakeupCredit : Entity
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid SourceSessionId { get; set; }
    public MakeupCreditStatus Status { get; set; }
    public CreatedReason CreatedReason { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Guid? UsedSessionId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Profile StudentProfile { get; set; } = null!;
    public Session SourceSession { get; set; } = null!;
    public Session? UsedSession { get; set; }
    public ICollection<MakeupAllocation> MakeupAllocations { get; set; } = new List<MakeupAllocation>();
}
