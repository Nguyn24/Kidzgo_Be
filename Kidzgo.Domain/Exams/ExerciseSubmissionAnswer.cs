using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Exams;

public class ExerciseSubmissionAnswer : Entity
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public Guid QuestionId { get; set; }
    public string Answer { get; set; } = null!; // Student's answer
    public bool IsCorrect { get; set; } // Whether answer is correct (for auto-graded questions)
    public decimal? PointsAwarded { get; set; } // Points awarded for this answer
    public string? TeacherFeedback { get; set; } // Teacher feedback (for writing questions)

    // Navigation properties
    public ExerciseSubmission Submission { get; set; } = null!;
    public ExerciseQuestion Question { get; set; } = null!;
}

