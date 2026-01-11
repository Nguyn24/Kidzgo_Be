using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Exams;

public class ExamSubmissionAnswer : Entity
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public Guid QuestionId { get; set; }
    public string Answer { get; set; } = null!; // Student's answer
    public bool IsCorrect { get; set; } // Whether answer is correct (for auto-graded questions)
    public decimal? PointsAwarded { get; set; } // Points awarded for this answer
    public string? TeacherFeedback { get; set; } // Teacher feedback (for writing questions)
    public DateTime? AnsweredAt { get; set; } // Thời gian trả lời

    // Navigation properties
    public ExamSubmission Submission { get; set; } = null!;
    public ExamQuestion Question { get; set; } = null!;
}

