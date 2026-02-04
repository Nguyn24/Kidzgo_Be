using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Exercises.Submissions.GetAnswerDetail;

/// <summary>
/// UC-150: Xem chi tiết từng câu trả lời
/// </summary>
public sealed class GetAnswerDetailQuery : IQuery<GetAnswerDetailResponse>
{
    public Guid SubmissionId { get; init; }
    public Guid QuestionId { get; init; }
}


