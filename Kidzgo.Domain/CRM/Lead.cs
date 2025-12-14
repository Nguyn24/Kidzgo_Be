using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.CRM;

public class Lead : Entity
{
    public Guid Id { get; set; }
    public LeadSource Source { get; set; }
    public string? Campaign { get; set; }
    public string ContactName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? ZaloId { get; set; }
    public string? Email { get; set; }
    public Guid? BranchPreference { get; set; }
    public string? ProgramInterest { get; set; }
    public string? Notes { get; set; }
    public LeadStatus Status { get; set; }
    public Guid? OwnerStaffId { get; set; }
    public DateTime? FirstResponseAt { get; set; }
    public int TouchCount { get; set; }
    public DateTime? NextActionAt { get; set; }
    public Guid? ConvertedStudentProfileId { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Branch? BranchPreferenceNavigation { get; set; }
    public User? OwnerStaffUser { get; set; }
    public Profile? ConvertedStudentProfile { get; set; }
    public ICollection<PlacementTest> PlacementTests { get; set; } = new List<PlacementTest>();
    public ICollection<LeadActivity> LeadActivities { get; set; } = new List<LeadActivity>();
}
