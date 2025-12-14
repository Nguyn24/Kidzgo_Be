using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.LessonPlans;

public class LessonPlanTemplate : Entity
{
    public Guid Id { get; set; }
    public Guid ProgramId { get; set; }
    public string? Level { get; set; }
    public int SessionIndex { get; set; }
    public string? StructureJson { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Program Program { get; set; } = null!;
    public User? CreatedByUser { get; set; }
    public ICollection<LessonPlan> LessonPlans { get; set; } = new List<LessonPlan>();
}
