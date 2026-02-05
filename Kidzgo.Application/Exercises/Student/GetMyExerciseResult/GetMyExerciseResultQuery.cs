using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Exercises.Student.GetMyExerciseResult;

/// <summary>
/// Student xem điểm + feedback (tổng quan và theo từng câu).
/// </summary>
public sealed class GetMyExerciseResultQuery : IQuery<GetMyExerciseResultResponse>
{
    public Guid SubmissionId { get; init; }
}


