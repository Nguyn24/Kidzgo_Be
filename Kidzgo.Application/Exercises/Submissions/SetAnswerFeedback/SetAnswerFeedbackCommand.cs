using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exercises.Submissions.SetAnswerFeedback;

/// <summary>
/// UC-151: Nhập feedback cho câu trả lời
/// </summary>
public sealed class SetAnswerFeedbackCommand : ICommand
{
    public Guid SubmissionId { get; init; }
    public Guid QuestionId { get; init; }
    public string? TeacherFeedback { get; init; }
}


