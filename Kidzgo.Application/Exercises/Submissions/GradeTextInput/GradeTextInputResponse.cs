namespace Kidzgo.Application.Exercises.Submissions.GradeTextInput;

public sealed class GradeTextInputResponse
{
    public Guid SubmissionId { get; init; }
    public decimal? Score { get; init; }
    public int GradedAnswerCount { get; init; }
}


