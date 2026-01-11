using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exams.StartExamSubmission;

public sealed class StartExamSubmissionCommand : ICommand<StartExamSubmissionResponse>
{
    public Guid ExamId { get; init; }
    public Guid StudentProfileId { get; init; }
}


