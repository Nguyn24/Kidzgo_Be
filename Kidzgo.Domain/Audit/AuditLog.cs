using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Audit;

public class AuditLog : Entity
{
    public Guid Id { get; set; }
    public Guid? ActorUserId { get; set; }
    public Guid? ActorProfileId { get; set; }
    public string Action { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public Guid? EntityId { get; set; }
    public string? DataBefore { get; set; }
    public string? DataAfter { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User? ActorUser { get; set; }
    public Profile? ActorProfile { get; set; }
}
