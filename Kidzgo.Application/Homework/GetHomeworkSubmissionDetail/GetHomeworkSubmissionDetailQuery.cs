using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Homework.GetHomeworkSubmissionDetail;

public sealed class GetHomeworkSubmissionDetailQuery : IQuery<GetHomeworkSubmissionDetailResponse>
{
    public Guid HomeworkStudentId { get; init; }
}

