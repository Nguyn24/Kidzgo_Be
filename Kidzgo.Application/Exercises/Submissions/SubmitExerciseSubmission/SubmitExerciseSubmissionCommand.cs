using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exercises.Submissions.SubmitExerciseSubmission;

/// <summary>
/// Student submits an exercise submission. Triggers UC-147 auto-grade for multiple choice.
/// </summary>
public sealed class SubmitExerciseSubmissionCommand : ICommand<SubmitExerciseSubmissionResponse>
{
    public Guid SubmissionId { get; init; }
}


