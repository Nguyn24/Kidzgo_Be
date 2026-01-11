using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exams.GetExamQuestions;

public sealed class GetExamQuestionsQuery : IQuery<GetExamQuestionsResponse>
{
    public Guid ExamId { get; init; }
}


