using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exercises.Submissions.AutoGradeMultipleChoice;

/// <summary>
/// UC-147: Tự động chấm Multiple Choice
/// </summary>
public sealed class AutoGradeMultipleChoiceCommand : ICommand<AutoGradeMultipleChoiceResponse>
{
    public Guid SubmissionId { get; init; }
}


