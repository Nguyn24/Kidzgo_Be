namespace Kidzgo.Application.Exercises.Submissions.SubmitExerciseSubmission;

public sealed class SubmitExerciseSubmissionResponse
{
    public Guid SubmissionId { get; init; }
    public DateTime SubmittedAt { get; init; }
    public decimal? Score { get; init; }
}


