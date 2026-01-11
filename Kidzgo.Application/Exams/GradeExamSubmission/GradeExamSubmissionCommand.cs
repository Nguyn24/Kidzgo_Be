using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exams.GradeExamSubmission;

public sealed class GradeExamSubmissionCommand : ICommand<GradeExamSubmissionResponse>
{
    public Guid SubmissionId { get; init; }
    public decimal? FinalScore { get; init; }
    public string? TeacherComment { get; init; }
    public List<GradeAnswerItem>? AnswerGrades { get; init; } // For grading individual answers
}

public sealed class GradeAnswerItem
{
    public Guid QuestionId { get; init; }
    public decimal? PointsAwarded { get; init; }
    public string? TeacherFeedback { get; init; }
}


