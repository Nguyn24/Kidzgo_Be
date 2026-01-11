using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Exams.GetExamResults;

public sealed class GetExamResultsQuery : IQuery<GetExamResultsResponse>, IPageableQuery
{
    public Guid? ExamId { get; init; }
    public Guid? StudentProfileId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

