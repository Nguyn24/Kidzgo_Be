using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.CRM;

public class LeadActivity : Entity
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public ActivityType ActivityType { get; set; }
    public string? Content { get; set; }
    public DateTime? NextActionAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Lead Lead { get; set; } = null!;
    public User? CreatedByUser { get; set; }
}
