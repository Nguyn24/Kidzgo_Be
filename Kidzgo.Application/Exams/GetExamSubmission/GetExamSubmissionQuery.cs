using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exams.GetExamSubmission;

public sealed class GetExamSubmissionQuery : IQuery<GetExamSubmissionResponse>
{
    public Guid SubmissionId { get; init; }
    public bool IncludeAnswers { get; init; } = true; // Include submission answers
    public bool ShowCorrectAnswers { get; init; } = false; // Show correct answers (for teacher/admin)
}


