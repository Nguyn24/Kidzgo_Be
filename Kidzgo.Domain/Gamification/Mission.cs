using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.LessonPlans;

namespace Kidzgo.Domain.Gamification;

public class Mission : Entity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public MissionScope Scope { get; set; }
    public Guid? TargetClassId { get; set; }
    public string? TargetGroup { get; set; }
    public MissionType MissionType { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public int? RewardStars { get; set; }
    public int? RewardExp { get; set; } // Experience points reward
    public int? TotalQuestions { get; set; } // Total number of questions for question-based missions
    public decimal? ProgressPerQuestion { get; set; } // Percentage progress per question (e.g., 10% per question if 10 questions)
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Class? TargetClass { get; set; }
    public User? CreatedByUser { get; set; }
    public ICollection<MissionProgress> MissionProgresses { get; set; } = new List<MissionProgress>();
    public ICollection<HomeworkAssignment> HomeworkAssignments { get; set; } = new List<HomeworkAssignment>();
}
