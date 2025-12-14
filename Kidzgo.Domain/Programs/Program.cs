using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.LessonPlans;

namespace Kidzgo.Domain.Programs;

public class Program : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Level { get; set; }
    public int TotalSessions { get; set; }
    public decimal DefaultTuitionAmount { get; set; }
    public decimal UnitPriceSession { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public ICollection<Class> Classes { get; set; } = new List<Class>();
    public ICollection<TuitionPlan> TuitionPlans { get; set; } = new List<TuitionPlan>();
    public ICollection<LessonPlanTemplate> LessonPlanTemplates { get; set; } = new List<LessonPlanTemplate>();
}
