using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exercises.Submissions.GradeTextInput;

/// <summary>
/// UC-148: Teacher chấm Text Input (Writing)
/// </summary>
public sealed class GradeTextInputCommand : ICommand<GradeTextInputResponse>
{
    public Guid SubmissionId { get; init; }
    public IReadOnlyList<TextAnswerGradeItem> AnswerGrades { get; init; } = Array.Empty<TextAnswerGradeItem>();
}

public sealed class TextAnswerGradeItem
{
    public Guid QuestionId { get; init; }
    public decimal PointsAwarded { get; init; }
    public string? TeacherFeedback { get; init; }
}


