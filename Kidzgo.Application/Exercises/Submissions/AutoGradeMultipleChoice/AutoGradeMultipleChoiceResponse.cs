namespace Kidzgo.Application.Exercises.Submissions.AutoGradeMultipleChoice;

public sealed class AutoGradeMultipleChoiceResponse
{
    public Guid SubmissionId { get; init; }
    public decimal? Score { get; init; }
    public int AutoGradedAnswerCount { get; init; }
}


