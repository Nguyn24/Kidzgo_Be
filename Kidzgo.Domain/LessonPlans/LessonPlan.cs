using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.LessonPlans;

public class LessonPlan : Entity
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid? TemplateId { get; set; }
    public string? PlannedContent { get; set; }
    public string? ActualContent { get; set; }
    public string? ActualHomework { get; set; }
    public string? TeacherNotes { get; set; }
    public Guid? SubmittedBy { get; set; }
    public DateTime? SubmittedAt { get; set; }

    // Navigation properties
    public Session Session { get; set; } = null!;
    public LessonPlanTemplate? Template { get; set; }
    public User? SubmittedByUser { get; set; }
}
