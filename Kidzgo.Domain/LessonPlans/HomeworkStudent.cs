using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.LessonPlans;

public class HomeworkStudent : Entity
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid StudentProfileId { get; set; }
    public HomeworkStatus Status { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? GradedAt { get; set; }
    public decimal? Score { get; set; }
    public string? TeacherFeedback { get; set; }
    public string? AiFeedback { get; set; }
    
    // Student submission fields
    public string? TextAnswer { get; set; }
    public string? AttachmentUrl { get; set; }

    // Navigation properties
    public HomeworkAssignment Assignment { get; set; } = null!;
    public Profile StudentProfile { get; set; } = null!;
}
