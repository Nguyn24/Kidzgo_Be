using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Schools;

namespace Kidzgo.Domain.Programs;

public class Program : Entity
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public decimal DefaultTuitionAmount { get; set; }
    public decimal UnitPriceSession { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsMakeup { get; set; }
    public bool IsSupplementary { get; set; }
    public Guid? DefaultMakeupClassId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Branch Branch { get; set; } = null!;
    public ICollection<Class> Classes { get; set; } = new List<Class>();
    public Class? DefaultMakeupClass { get; set; }
    public ICollection<TuitionPlan> TuitionPlans { get; set; } = new List<TuitionPlan>();
    public ICollection<LessonPlanTemplate> LessonPlanTemplates { get; set; } = new List<LessonPlanTemplate>();
    public ICollection<QuestionBankItem> QuestionBankItems { get; set; } = new List<QuestionBankItem>();
    public ICollection<PlacementTest> PrimaryPlacementTestRecommendations { get; set; } = new List<PlacementTest>();
    public ICollection<PlacementTest> SecondaryPlacementTestRecommendations { get; set; } = new List<PlacementTest>();
}
