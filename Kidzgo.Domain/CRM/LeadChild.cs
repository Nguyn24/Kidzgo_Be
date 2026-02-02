using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.CRM;

public class LeadChild : Entity
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public string ChildName { get; set; } = null!;
    public DateTime? Dob { get; set; }
    public string? Gender { get; set; }
    public string? ProgramInterest { get; set; }
    public string? Notes { get; set; }
    public LeadChildStatus Status { get; set; }
    public Guid? ConvertedStudentProfileId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Lead Lead { get; set; } = null!;
    public Profile? ConvertedStudentProfile { get; set; }
    public ICollection<PlacementTest> PlacementTests { get; set; } = new List<PlacementTest>();
}

