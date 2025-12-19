using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Exams;

public class Exercise : Entity
{
    public Guid Id { get; set; }
    public Guid? ClassId { get; set; } // Optional: can be assigned to a specific class
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public ExerciseType ExerciseType { get; set; } // READING, LISTENING, WRITING
    public Guid CreatedBy { get; set; } // Teacher/Admin who created
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Class? Class { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public ICollection<ExerciseQuestion> Questions { get; set; } = new List<ExerciseQuestion>();
    public ICollection<ExerciseSubmission> Submissions { get; set; } = new List<ExerciseSubmission>();
}

public enum ExerciseType
{
    Reading,
    Listening,
    Writing
}

