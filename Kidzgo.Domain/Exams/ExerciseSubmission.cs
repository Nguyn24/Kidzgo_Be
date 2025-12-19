using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Exams;

public class ExerciseSubmission : Entity
{
    public Guid Id { get; set; }
    public Guid ExerciseId { get; set; }
    public Guid StudentProfileId { get; set; }
    public string? Answers { get; set; } // JSON object: { questionId: answer }
    public decimal? Score { get; set; } // Total score
    public DateTime SubmittedAt { get; set; }
    public DateTime? GradedAt { get; set; }
    public Guid? GradedBy { get; set; } // Teacher who graded (for writing exercises)

    // Navigation properties
    public Exercise Exercise { get; set; } = null!;
    public Profile StudentProfile { get; set; } = null!;
    public User? GradedByUser { get; set; }
    public ICollection<ExerciseSubmissionAnswer> SubmissionAnswers { get; set; } = new List<ExerciseSubmissionAnswer>();
}

