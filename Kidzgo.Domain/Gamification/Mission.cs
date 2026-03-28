using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Homework;

namespace Kidzgo.Domain.Gamification;

public class Mission : Entity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public MissionScope Scope { get; set; }
    public Guid? TargetClassId { get; set; }
    public Guid? TargetStudentId { get; set; } // Chi duy nhat cho Scope.Student
    public List<Guid>? TargetGroup { get; set; } // Danh sach student IDs cho Scope.Group
    public MissionType MissionType { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public int? RewardStars { get; set; }
    public int? RewardExp { get; set; }
    public int? TotalRequired { get; set; } // Target value for streak missions
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Class? TargetClass { get; set; }
    public User? CreatedByUser { get; set; }
    public ICollection<MissionProgress> MissionProgresses { get; set; } = new List<MissionProgress>();
    public ICollection<HomeworkAssignment> HomeworkAssignments { get; set; } = new List<HomeworkAssignment>();
}
