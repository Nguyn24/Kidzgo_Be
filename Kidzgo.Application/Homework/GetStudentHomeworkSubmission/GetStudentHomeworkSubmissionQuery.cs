using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Homework.GetStudentHomeworkSubmission;

public sealed class GetStudentHomeworkSubmissionQuery : IQuery<GetStudentHomeworkSubmissionResponse>
{
    public Guid HomeworkStudentId { get; init; }
    public int? AttemptNumber { get; init; }
}

