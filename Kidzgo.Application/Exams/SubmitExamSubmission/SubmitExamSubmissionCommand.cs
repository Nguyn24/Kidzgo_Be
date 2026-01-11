using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exams.SubmitExamSubmission;

public sealed class SubmitExamSubmissionCommand : ICommand<SubmitExamSubmissionResponse>
{
    public Guid SubmissionId { get; init; }
    public bool IsAutoSubmit { get; init; } = false; // True if auto-submitted due to time limit
}


